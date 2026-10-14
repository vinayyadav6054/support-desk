using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Dashboard;
using SupportDesk.Domain;

namespace SupportDesk.Infrastructure.Dashboard;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;
    public DashboardService(AppDbContext db) => _db = db;

    public async Task<DashboardDto> GetAsync(int agentId)
    {
        var tickets = _db.Tickets.AsNoTracking();

        // Status totals
        var statusCounts = await tickets
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        int CountOf(TicketStatus s) =>
            statusCounts.Where(x => x.Status == s).Select(x => x.Count).FirstOrDefault();

        var open = CountOf(TicketStatus.Open);
        var inProgress = CountOf(TicketStatus.InProgress);
        var resolved = CountOf(TicketStatus.Resolved);
        var closed = CountOf(TicketStatus.Closed);

        // Workload numbers (active = Open or InProgress)
        var needsAgent = await tickets.CountAsync(t =>
            !t.BotActive && t.AssignedAgentId == null &&
            (t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress));

        var escalatedOpen = await tickets.CountAsync(t =>
            t.EscalatedFromBot &&
            (t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress));

        var botHandling = await tickets.CountAsync(t => t.BotActive);

        var myOpen = await tickets.CountAsync(t =>
            t.AssignedAgentId == agentId &&
            (t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress));

        var resolvedByBot = await tickets.CountAsync(t =>
            t.Status == TicketStatus.Resolved &&
            t.Messages.Any(m => m.SenderType == SenderType.Bot) &&
            !t.Messages.Any(m => m.SenderType == SenderType.Agent));

        // Priority breakdown (always return all four, even with zero)
        var priorityCounts = await tickets
            .GroupBy(t => t.Priority)
            .Select(g => new { Priority = g.Key, Count = g.Count() })
            .ToListAsync();

        var byPriority = Enum.GetValues<TicketPriority>()
            .Select(p => new PriorityCount(
                p.ToString(),
                priorityCounts.Where(x => x.Priority == p).Select(x => x.Count).FirstOrDefault()))
            .ToList();

        // Tickets created per day over the last 7 days (UTC days)
        var from = DateTime.UtcNow.Date.AddDays(-6);
        var perDay = await tickets
            .Where(t => t.CreatedAt >= from)
            .GroupBy(t => t.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        var last7Days = Enumerable.Range(0, 7)
            .Select(i => from.AddDays(i))
            .Select(d => new DayCount(
                d.ToString("yyyy-MM-dd"),
                perDay.Where(x => x.Date == d).Select(x => x.Count).FirstOrDefault()))
            .ToList();

        // Average time to the first agent reply, last 30 days
        var since = DateTime.UtcNow.AddDays(-30);
        var firstReplies = await tickets
            .Where(t => t.CreatedAt >= since)
            .Select(t => new
            {
                t.CreatedAt,
                FirstAgentReply = t.Messages
                    .Where(m => m.SenderType == SenderType.Agent)
                    .Min(m => (DateTime?)m.SentAt)
            })
            .Where(x => x.FirstAgentReply != null)
            .ToListAsync();

        double? avgMinutes = firstReplies.Count == 0
            ? null
            : Math.Round(firstReplies.Average(x => (x.FirstAgentReply!.Value - x.CreatedAt).TotalMinutes), 1);

        return new DashboardDto(
            Total: open + inProgress + resolved + closed,
            Open: open, InProgress: inProgress, Resolved: resolved, Closed: closed,
            NeedsAgent: needsAgent, EscalatedOpen: escalatedOpen, BotHandling: botHandling,
            ResolvedByBot: resolvedByBot, MyOpen: myOpen,
            ByPriority: byPriority, Last7Days: last7Days,
            AvgFirstResponseMinutes: avgMinutes);
    }
}