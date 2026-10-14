using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Tickets;
using SupportDesk.Domain;

namespace SupportDesk.Infrastructure.Tickets;

public class TicketService : ITicketService
{
    private readonly AppDbContext _db;
    public TicketService(AppDbContext db) => _db = db;

    public async Task<TicketDetailDto> CreateAsync(int userId, CreateTicketRequest request)
    {
        var ticket = new Ticket
        {
            Subject = request.Subject.Trim(),
            Description = request.Description?.Trim(),
            Priority = request.Priority,
            CustomerId = userId
        };

        // The description becomes the first message in the conversation
        if (!string.IsNullOrWhiteSpace(ticket.Description))
        {
            ticket.Messages.Add(new Message
            {
                Content = ticket.Description,
                SenderType = SenderType.Customer,
                SenderId = userId
            });
        }

        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync();

        return (await GetDetailAsync(ticket.Id))!;
    }

    public async Task<List<TicketSummaryDto>> GetListAsync(int userId, UserRole role)
    {
        var query = _db.Tickets.AsNoTracking().AsQueryable();

        if (role == UserRole.Customer)
            query = query.Where(t => t.CustomerId == userId);

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TicketSummaryDto(
                t.Id, t.Subject, t.Status.ToString(), t.Priority.ToString(),
                t.Customer.FullName,
                t.AssignedAgent != null ? t.AssignedAgent.FullName : null,
                t.CreatedAt, t.UpdatedAt))
            .ToListAsync();
    }

    public async Task<TicketDetailDto?> GetByIdAsync(int ticketId, int userId, UserRole role)
    {
        var dto = await GetDetailAsync(ticketId);
        if (dto is null) return null;

        if (role == UserRole.Customer && dto.CustomerId != userId)
            return null;

        return dto;
    }

    public async Task<TicketDetailDto?> UpdateAsync(int ticketId, UpdateTicketRequest request)
    {
        var ticket = await _db.Tickets.FindAsync(ticketId);
        if (ticket is null) return null;

        if (request.Status.HasValue) ticket.Status = request.Status.Value;
        if (request.Priority.HasValue) ticket.Priority = request.Priority.Value;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return await GetDetailAsync(ticketId);
    }

    public async Task<TicketDetailDto?> AssignToMeAsync(int ticketId, int agentId)
    {
        var ticket = await _db.Tickets.FindAsync(ticketId);
        if (ticket is null) return null;

        ticket.AssignedAgentId = agentId;
        if (ticket.Status == TicketStatus.Open) ticket.Status = TicketStatus.InProgress;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return await GetDetailAsync(ticketId);
    }

    public async Task<MessageDto?> AddMessageAsync(int ticketId, int userId, UserRole role, AddMessageRequest request)
    {
        var ticket = await _db.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket is null) return null;
        if (role == UserRole.Customer && ticket.CustomerId != userId) return null;

        var sender = await _db.Users.AsNoTracking().FirstAsync(u => u.Id == userId);

        var message = new Message
        {
            TicketId = ticketId,
            Content = request.Content.Trim(),
            SenderId = userId,
            SenderType = role == UserRole.Customer ? SenderType.Customer : SenderType.Agent
        };

        _db.Messages.Add(message);
        await _db.Tickets
            .Where(t => t.Id == ticketId)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.UpdatedAt, DateTime.UtcNow));
        await _db.SaveChangesAsync();

        return new MessageDto(message.Id, message.Content, message.SenderType.ToString(),
            sender.FullName, message.SentAt);
    }

    private Task<TicketDetailDto?> GetDetailAsync(int ticketId) =>
        _db.Tickets.AsNoTracking()
            .Where(t => t.Id == ticketId)
            .Select(t => new TicketDetailDto(
                t.Id, t.Subject, t.Description, t.Status.ToString(), t.Priority.ToString(),
                t.CustomerId, t.Customer.FullName,
                t.AssignedAgentId, t.AssignedAgent != null ? t.AssignedAgent.FullName : null,
                t.CreatedAt, t.UpdatedAt,
                t.Messages.OrderBy(m => m.SentAt)
                    .Select(m => new MessageDto(m.Id, m.Content, m.SenderType.ToString(),
                        m.Sender != null ? m.Sender.FullName : "Support Bot", m.SentAt))
                    .ToList()))
            .FirstOrDefaultAsync();

    public Task<int?> GetCustomerIdAsync(int ticketId) =>
    _db.Tickets.AsNoTracking()
        .Where(t => t.Id == ticketId)
        .Select(t => (int?)t.CustomerId)
        .FirstOrDefaultAsync();
}