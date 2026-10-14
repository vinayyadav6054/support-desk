using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.Application.Users;

namespace SupportDesk.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserAdminService _users;
    public UsersController(IUserAdminService users) => _users = users;

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _users.GetAllAsync());

    [HttpPut("{id:int}/role")]
    public async Task<IActionResult> ChangeRole(int id, ChangeRoleRequest request)
    {
        var result = await _users.ChangeRoleAsync(id, request.Role, UserId);

        if (result.NotFound) return NotFound();
        if (result.Error is not null) return BadRequest(new { message = result.Error });
        return Ok(result.User);
    }
}