using System.ComponentModel.DataAnnotations;
using SupportDesk.Domain;

namespace SupportDesk.Application.Tickets;

public record CreateTicketRequest(
    [Required, MaxLength(200)] string Subject,
    string? Description,
    TicketPriority Priority = TicketPriority.Medium);

public record UpdateTicketRequest(TicketStatus? Status, TicketPriority? Priority);

public record AddMessageRequest([Required] string Content);

public record MessageDto(int Id, string Content, string SenderType, string? SenderName, DateTime SentAt);

public record TicketSummaryDto(
    int Id, string Subject, string Status, string Priority,
    string CustomerName, string? AssignedAgentName,
    DateTime CreatedAt, DateTime? UpdatedAt,
    bool BotActive, bool EscalatedFromBot);

public record TicketDetailDto(
    int Id, string Subject, string? Description, string Status, string Priority,
    int CustomerId, string CustomerName, int? AssignedAgentId, string? AssignedAgentName,
    DateTime CreatedAt, DateTime? UpdatedAt, List<MessageDto> Messages,
    bool BotActive, bool EscalatedFromBot);