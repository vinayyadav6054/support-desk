using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Chatbot;
using SupportDesk.Domain;

namespace SupportDesk.Infrastructure.Chatbot;

public class FaqChatbotService : IChatbotService
{
    private const double MinScore = 0.4;

    private static readonly HashSet<string> Stopwords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a","an","the","i","me","my","we","our","you","your","to","of","in","on","at","is","are","was",
        "were","be","it","its","this","that","and","or","for","with","can","could","would","please",
        "help","need","want","how","what","why","when","hi","hello","hey","thanks","thank"
    };

    private readonly AppDbContext _db;
    public FaqChatbotService(AppDbContext db) => _db = db;

    public async Task<BotReply> GetReplyAsync(string message)
    {
        var query = Tokenize(message);
        if (query.Count == 0) return new BotReply(false, string.Empty, null, 0);

        var articles = await _db.KnowledgeArticles
            .AsNoTracking()
            .Where(a => a.IsActive)
            .ToListAsync();

        KnowledgeArticle? best = null;
        double bestScore = 0;

        foreach (var article in articles)
        {
            var keywords = Tokenize(article.Keywords);
            var title = Tokenize(article.Title);

            double points = 0;
            int keywordHits = 0;

            foreach (var token in query)
            {
                if (keywords.Contains(token)) { points += 1.0; keywordHits++; }
                else if (title.Contains(token)) { points += 0.6; }
            }

            // At least one real keyword must match, otherwise title words alone could mislead
            if (keywordHits == 0) continue;

            // Long messages shouldn't be punished: divide by at most 3 words
            var score = Math.Min(1.0, points / Math.Min(query.Count, 3));

            if (score > bestScore)
            {
                best = article;
                bestScore = score;
            }
        }

        if (best is null) return new BotReply(false, string.Empty, null, 0);

        var matched = bestScore >= MinScore;
        return new BotReply(matched, matched ? best.Answer : string.Empty, best.Id, bestScore);
    }

    private static HashSet<string> Tokenize(string text) =>
        Regex.Matches(
                text.ToLowerInvariant().Replace("'", "").Replace("\u2019", ""),
                "[a-z0-9]+")
            .Select(m => m.Value)
            .Where(t => t.Length > 1 && !Stopwords.Contains(t))
            .Select(Stem)
            .ToHashSet();

    // Very light stemming so "invoices" matches "invoice" and "billing" matches "bill"
    private static string Stem(string t)
    {
        if (t.Length > 5 && t.EndsWith("ing")) return t[..^3];
        if (t.Length > 4 && t.EndsWith("ed")) return t[..^2];
        if (t.Length > 3 && t.EndsWith("s") && !t.EndsWith("ss")) return t[..^1];
        return t;
    }
}