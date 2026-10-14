using SupportDesk.Application.Tickets;

namespace SupportDesk.Application.Chatbot;

// Messages: new bot messages to broadcast. TicketChanged: status/bot state changed.
public record BotOutcome(List<MessageDto> Messages, bool TicketChanged);

public interface ITicketBotService
{
    Task<BotOutcome> HandleCustomerMessageAsync(int ticketId, string text);

    // Return null when the ticket doesn't exist or isn't the caller's
    Task<BotOutcome?> CustomerResolvedAsync(int ticketId, int userId);
    Task<BotOutcome?> CustomerEscalatedAsync(int ticketId, int userId);

    // True if the bot was active and has now stepped aside
    Task<bool> AgentTookOverAsync(int ticketId);
}