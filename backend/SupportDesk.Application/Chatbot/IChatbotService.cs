namespace SupportDesk.Application.Chatbot;

public record BotReply(bool Matched, string Text, int? ArticleId, double Score);

public interface IChatbotService
{
    // The swap point: FAQ matching today, an LLM in phase 7
    Task<BotReply> GetReplyAsync(string message);
}