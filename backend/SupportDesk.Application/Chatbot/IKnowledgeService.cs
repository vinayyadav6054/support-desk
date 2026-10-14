using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportDesk.Application.Chatbot;

public interface IKnowledgeService
{
    Task<List<ArticleDto>> GetAllAsync();
    Task<ArticleDto> CreateAsync(SaveArticleRequest request);
    Task<ArticleDto?> UpdateAsync(int id, SaveArticleRequest request);
    Task<bool> DeleteAsync(int id);
    Task<TestBotResponse> TestAsync(string message);
}
