using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Chatbot;
using SupportDesk.Domain;

namespace SupportDesk.Infrastructure.Chatbot;

public class KnowledgeService : IKnowledgeService
{
    private readonly AppDbContext _db;
    private readonly IChatbotService _bot;

    public KnowledgeService(AppDbContext db, IChatbotService bot)
    {
        _db = db;
        _bot = bot;
    }

    public Task<List<ArticleDto>> GetAllAsync() =>
        _db.KnowledgeArticles.AsNoTracking()
            .OrderBy(a => a.Title)
            .Select(a => new ArticleDto(a.Id, a.Title, a.Keywords, a.Answer, a.IsActive))
            .ToListAsync();

    public async Task<ArticleDto> CreateAsync(SaveArticleRequest r)
    {
        var article = new KnowledgeArticle
        {
            Title = r.Title.Trim(),
            Keywords = r.Keywords.Trim(),
            Answer = r.Answer.Trim(),
            IsActive = r.IsActive
        };
        _db.KnowledgeArticles.Add(article);
        await _db.SaveChangesAsync();
        return ToDto(article);
    }

    public async Task<ArticleDto?> UpdateAsync(int id, SaveArticleRequest r)
    {
        var article = await _db.KnowledgeArticles.FindAsync(id);
        if (article is null) return null;

        article.Title = r.Title.Trim();
        article.Keywords = r.Keywords.Trim();
        article.Answer = r.Answer.Trim();
        article.IsActive = r.IsActive;
        await _db.SaveChangesAsync();
        return ToDto(article);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var article = await _db.KnowledgeArticles.FindAsync(id);
        if (article is null) return false;

        _db.KnowledgeArticles.Remove(article);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<TestBotResponse> TestAsync(string message)
    {
        var reply = await _bot.GetReplyAsync(message);

        string? title = null;
        if (reply.ArticleId is not null)
        {
            title = await _db.KnowledgeArticles
                .Where(a => a.Id == reply.ArticleId)
                .Select(a => a.Title)
                .FirstOrDefaultAsync();
        }

        return new TestBotResponse(reply.Matched, reply.Text, title, Math.Round(reply.Score, 2));
    }

    private static ArticleDto ToDto(KnowledgeArticle a) =>
        new(a.Id, a.Title, a.Keywords, a.Answer, a.IsActive);
}