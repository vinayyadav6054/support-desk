using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SupportDesk.Application.Tickets;
using SupportDesk.Domain;

namespace SupportDesk.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    public const string StaffGroup = "staff";
    public static string TicketGroup(int ticketId) => $"ticket-{ticketId}";
    public static string UserGroup(int userId) => $"user-{userId}";

    private const string JoinedKey = "joined-tickets";
    private readonly ITicketService _tickets;

    public ChatHub(ITicketService tickets) => _tickets = tickets;

    private int UserId => int.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string UserName => Context.User!.FindFirstValue(ClaimTypes.Name) ?? "Someone";
    private UserRole Role => Enum.Parse<UserRole>(Context.User!.FindFirstValue(ClaimTypes.Role)!);

    private HashSet<int> Joined =>
        (HashSet<int>)(Context.Items[JoinedKey] ??= new HashSet<int>());

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(UserId));
        if (Role != UserRole.Customer)
            await Groups.AddToGroupAsync(Context.ConnectionId, StaffGroup);

        await base.OnConnectedAsync();
    }

    // Returns false if the ticket doesn't exist or the user isn't allowed to see it
    public async Task<bool> JoinTicket(int ticketId)
    {
        var ticket = await _tickets.GetByIdAsync(ticketId, UserId, Role);
        if (ticket is null) return false;

        await Groups.AddToGroupAsync(Context.ConnectionId, TicketGroup(ticketId));
        Joined.Add(ticketId);
        return true;
    }

    public async Task LeaveTicket(int ticketId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, TicketGroup(ticketId));
        Joined.Remove(ticketId);
    }

    public async Task Typing(int ticketId, bool isTyping)
    {
        // Only allow typing events for tickets this connection has legitimately joined
        if (!Joined.Contains(ticketId)) return;

        await Clients.OthersInGroup(TicketGroup(ticketId))
            .SendAsync("Typing", new { ticketId, name = UserName, isTyping });
    }
}