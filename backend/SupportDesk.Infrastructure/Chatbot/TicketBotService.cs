using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Chatbot;
using SupportDesk.Application.Tickets;
using SupportDesk.Domain;
using System;

namespace SupportDesk.Infrastructure.Chatbot;

public class TicketBotService : ITicketBotService
{
    private const int MaxBotAnswers = 3;
    private const string BotName = "Support Bot";

    private readonly AppDbContext _db;
    private readonly IChatbotService _chatbot;

    public TicketBotService(AppDbContext db, IChatbotService chatbot)
    {
        _db = db;
        _chatbot = chatbot;
    }

    public async Task<BotOutcome> HandleCustomerMessageAsync(int ticketId, string text)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket is null || !ticket.BotActive) return NoChange();

        var answersSoFar = await _db.Messages
            .CountAsync(m => m.TicketId == ticketId && m.SenderType == SenderType.Bot);

        var reply = await _chatbot.GetReplyAsync(text);

        if (reply.Matched && answersSoFar < MaxBotAnswers)
        {
            var message = AddBotMessage(ticket, reply.Text + "\n\nDid this solve your problem?");
            await _db.SaveChangesAsync();
            return new BotOutcome(new List<MessageDto> { ToDto(message) }, false);
        }

        return await EscalateAsync(ticket,
            "I couldn't find a good answer for that, so I'm bringing in a human agent. Someone from our team will reply here shortly.");
    }

    public async Task<BotOutcome?> CustomerResolvedAsync(int ticketId, int userId)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId && t.CustomerId == userId);
        if (ticket is null) return null;
        if (!ticket.BotActive) return NoChange();

        ticket.BotActive = false;
        ticket.Status = TicketStatus.Resolved;
        var message = AddBotMessage(ticket, "Glad I could help! I've marked this ticket as resolved.");
        await _db.SaveChangesAsync();
        return new BotOutcome(new List<MessageDto> { ToDto(message) }, true);
    }

    public async Task<BotOutcome?> CustomerEscalatedAsync(int ticketId, int userId)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId && t.CustomerId == userId);
        if (ticket is null) return null;
        if (!ticket.BotActive) return NoChange();

        return await EscalateAsync(ticket,
            "No problem, I've passed this to our support team. An agent will reply here soon.");
    }

    public async Task<bool> AgentTookOverAsync(int ticketId)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket is null || !ticket.BotActive) return false;

        ticket.BotActive = false;
        ticket.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    private async Task<BotOutcome> EscalateAsync(Ticket ticket, string text)
    {
        ticket.BotActive = false;
        ticket.EscalatedFromBot = true;
        var message = AddBotMessage(ticket, text);
        await _db.SaveChangesAsync();
        return new BotOutcome(new List<MessageDto> { ToDto(message) }, true);
    }

    private Message AddBotMessage(Ticket ticket, string content)
    {
        var message = new Message
        {
            TicketId = ticket.Id,
            Content = content,
            SenderType = SenderType.Bot,
            SenderId = null
        };
        _db.Messages.Add(message);
        ticket.UpdatedAt = DateTime.UtcNow;
        return message;
    }

    private static BotOutcome NoChange() => new(new List<MessageDto>(), false);

    private static MessageDto ToDto(Message m) =>
        new(m.Id, m.Content, m.SenderType.ToString(), BotName, m.SentAt);
}