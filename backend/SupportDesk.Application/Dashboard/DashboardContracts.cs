using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportDesk.Application.Dashboard;

public record DayCount(string Date, int Count);
public record PriorityCount(string Priority, int Count);

public record DashboardDto(
    int Total, int Open, int InProgress, int Resolved, int Closed,
    int NeedsAgent,          // not handled by the bot, no agent assigned, still active
    int EscalatedOpen,       // escalated from the bot and still active
    int BotHandling,         // currently being answered by the bot
    int ResolvedByBot,       // resolved without any agent reply
    int MyOpen,              // active tickets assigned to the caller
    List<PriorityCount> ByPriority,
    List<DayCount> Last7Days,
    double? AvgFirstResponseMinutes);

public interface IDashboardService
{
    Task<DashboardDto> GetAsync(int agentId);
}