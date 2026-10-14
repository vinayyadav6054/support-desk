using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.Application.Chatbot;

namespace SupportDesk.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/kb")]
public class KnowledgeController : ControllerBase
{
    private readonly IKnowledgeService _kb;
    public KnowledgeController(IKnowledgeService kb) => _kb = kb;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _kb.GetAllAsync());

    [HttpPost]
    public async Task<IActionResult> Create(SaveArticleRequest request) =>
        Ok(await _kb.CreateAsync(request));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SaveArticleRequest request)
    {
        var article = await _kb.UpdateAsync(id, request);
        return article is null ? NotFound() : Ok(article);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) =>
        await _kb.DeleteAsync(id) ? Ok(new { deleted = true }) : NotFound();

    [HttpPost("test")]
    public async Task<IActionResult> Test(TestBotRequest request) =>
        Ok(await _kb.TestAsync(request.Message));
}