using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.Application.Tickets;
using SupportDesk.Domain;

namespace SupportDesk.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _tickets;
    public TicketsController(ITicketService tickets) => _tickets = tickets;

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private UserRole Role => Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);

    [HttpPost]
    public async Task<IActionResult> Create(CreateTicketRequest request)
    {
        var ticket = await _tickets.CreateAsync(UserId, request);
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
        return ticket is null ? NotFound() : Ok(ticket);
    }

    [HttpPost("{id:int}/assign-to-me")]
    [Authorize(Roles = "Agent,Admin")]
    public async Task<IActionResult> AssignToMe(int id)
    {
        var ticket = await _tickets.AssignToMeAsync(id, UserId);
        return ticket is null ? NotFound() : Ok(ticket);
    }

    [HttpPost("{id:int}/messages")]
    public async Task<IActionResult> AddMessage(int id, AddMessageRequest request)
    {
        var message = await _tickets.AddMessageAsync(id, UserId, Role, request);
        return message is null ? NotFound() : Ok(message);
    }
}