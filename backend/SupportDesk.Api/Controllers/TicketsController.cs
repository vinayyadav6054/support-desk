using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SupportDesk.Api.Hubs;
using SupportDesk.Application.Tickets;
using SupportDesk.Domain;

namespace SupportDesk.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _tickets;
    private readonly IHubContext<ChatHub> _hub;

    public TicketsController(ITicketService tickets, IHubContext<ChatHub> hub)
    {
        _tickets = tickets;
        _hub = hub;
    }

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private UserRole Role => Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);

    // Tells staff and the ticket's customer to refresh their ticket lists
    private Task NotifyListsAsync(int ticketId, int customerId) =>
        _hub.Clients.Groups(ChatHub.StaffGroup, ChatHub.UserGroup(customerId))
            .SendAsync("TicketChanged", ticketId);

    [HttpPost]
    public async Task<IActionResult> Create(CreateTicketRequest request)
    {
        var ticket = await _tickets.CreateAsync(UserId, request);
        await NotifyListsAsync(ticket.Id, ticket.CustomerId);
        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
    }

    [HttpGet]
    public async Task<IActionResult> GetList() =>
        Ok(await _tickets.GetListAsync(UserId, Role));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var ticket = await _tickets.GetByIdAsync(id, UserId, Role);
        return ticket is null ? NotFound() : Ok(ticket);
    }

    [HttpPatch("{id:int}")]
    [Authorize(Roles = "Agent,Admin")]
    public async Task<IActionResult> Update(int id, UpdateTicketRequest request)
    {
        var ticket = await _tickets.UpdateAsync(id, request);
        if (ticket is null) return NotFound();

        await _hub.Clients.Group(ChatHub.TicketGroup(id)).SendAsync("TicketUpdated", ticket);
        await NotifyListsAsync(id, ticket.CustomerId);
        return Ok(ticket);
    }

    [HttpPost("{id:int}/assign-to-me")]
    [Authorize(Roles = "Agent,Admin")]
    public async Task<IActionResult> AssignToMe(int id)
    {
        var ticket = await _tickets.AssignToMeAsync(id, UserId);
        if (ticket is null) return NotFound();

        await _hub.Clients.Group(ChatHub.TicketGroup(id)).SendAsync("TicketUpdated", ticket);
        await NotifyListsAsync(id, ticket.CustomerId);
        return Ok(ticket);
    }

    [HttpPost("{id:int}/messages")]
    public async Task<IActionResult> AddMessage(int id, AddMessageRequest request)
    {
        var message = await _tickets.AddMessageAsync(id, UserId, Role, request);
        if (message is null) return NotFound();

        await _hub.Clients.Group(ChatHub.TicketGroup(id)).SendAsync("ReceiveMessage", id, message);

        var customerId = await _tickets.GetCustomerIdAsync(id);
        if (customerId is not null) await NotifyListsAsync(id, customerId.Value);

        return Ok(message);
    }
}