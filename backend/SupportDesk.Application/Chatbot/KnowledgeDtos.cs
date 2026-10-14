using System.ComponentModel.DataAnnotations;

namespace SupportDesk.Application.Chatbot;

public record ArticleDto(int Id, string Title, string Keywords, string Answer, bool IsActive);

public record SaveArticleRequest(
    [Required, MaxLength(200)] string Title,
    [Required, MaxLength(500)] string Keywords,
    [Required, MaxLength(4000)] string Answer,
    bool IsActive = true);

public record TestBotRequest([Required] string Message);

public record TestBotResponse(bool Matched, string Reply, string? ArticleTitle, double Score);