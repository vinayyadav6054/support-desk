using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SupportDesk.Api.Hubs;
using SupportDesk.Application.Chatbot;
using SupportDesk.Application.Tickets;
using SupportDesk.Domain;

namespace SupportDesk.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _tickets;
    private readonly ITicketBotService _bot;
    private readonly IHubContext<ChatHub> _hub;

    public TicketsController(ITicketService tickets, ITicketBotService bot, IHubContext<ChatHub> hub)
    {
        _tickets = tickets;
        _bot = bot;
        _hub = hub;
    }

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private UserRole Role => Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);

    // ---------- realtime helpers ----------

    private Task NotifyListsAsync(int ticketId, int customerId) =>
        _hub.Clients.Groups(ChatHub.StaffGroup, ChatHub.UserGroup(customerId))
            .SendAsync("TicketChanged", ticketId);

    private async Task PublishTicketUpdatedAsync(TicketDetailDto ticket)
    {
        await _hub.Clients.Group(ChatHub.TicketGroup(ticket.Id)).SendAsync("TicketUpdated", ticket);
        await NotifyListsAsync(ticket.Id, ticket.CustomerId);
    }

    private async Task PublishBotOutcomeAsync(int ticketId, BotOutcome outcome)
    {
        foreach (var message in outcome.Messages)
            await _hub.Clients.Group(ChatHub.TicketGroup(ticketId)).SendAsync("ReceiveMessage", ticketId, message);

        if (outcome.TicketChanged)
        {
            var ticket = await _tickets.GetByIdAsync(ticketId, UserId, Role);
            if (ticket is not null) await PublishTicketUpdatedAsync(ticket);
        }
    }

    // ---------- endpoints ----------

    [HttpPost]
    public async Task<IActionResult> Create(CreateTicketRequest request)
    {
        var created = await _tickets.CreateAsync(UserId, request);

        // The bot answers the first message straight away
        await _bot.HandleCustomerMessageAsync(created.Id, $"{request.Subject} {request.Description}");

        var ticket = await _tickets.GetByIdAsync(created.Id, UserId, Role) ?? created;
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

        await PublishTicketUpdatedAsync(ticket);
        return Ok(ticket);
    }

    [HttpPost("{id:int}/assign-to-me")]
    [Authorize(Roles = "Agent,Admin")]
    public async Task<IActionResult> AssignToMe(int id)
    {
        var ticket = await _tickets.AssignToMeAsync(id, UserId);
        if (ticket is null) return NotFound();

        await PublishTicketUpdatedAsync(ticket);
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

        if (Role == UserRole.Customer)
        {
            // Bot replies if it's still handling this ticket
            await PublishBotOutcomeAsync(id, await _bot.HandleCustomerMessageAsync(id, request.Content));
        }
        else if (await _bot.AgentTookOverAsync(id))
        {
            // An agent replied, so the bot steps out
            var ticket = await _tickets.GetByIdAsync(id, UserId, Role);
            if (ticket is not null) await PublishTicketUpdatedAsync(ticket);
        }

        return Ok(message);
    }

    [HttpPost("{id:int}/bot/resolve")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> BotResolve(int id)
    {
        var outcome = await _bot.CustomerResolvedAsync(id, UserId);
        if (outcome is null) return NotFound();

        await PublishBotOutcomeAsync(id, outcome);
        return Ok(await _tickets.GetByIdAsync(id, UserId, Role));
    }

    [HttpPost("{id:int}/bot/escalate")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> BotEscalate(int id)
    {
        var outcome = await _bot.CustomerEscalatedAsync(id, UserId);
        if (outcome is null) return NotFound();

        await PublishBotOutcomeAsync(id, outcome);
        return Ok(await _tickets.GetByIdAsync(id, UserId, Role));
    }
}