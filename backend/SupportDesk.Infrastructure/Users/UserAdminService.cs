using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Users;
using SupportDesk.Domain;

namespace SupportDesk.Infrastructure.Users;

public class UserAdminService : IUserAdminService
{
    private readonly AppDbContext _db;
    public UserAdminService(AppDbContext db) => _db = db;

    public Task<List<UserDto>> GetAllAsync() =>
        _db.Users.AsNoTracking()
            .OrderBy(u => u.FullName)
            .Select(u => new UserDto(u.Id, u.FullName, u.Email, u.Role.ToString(), u.CreatedAt))
            .ToListAsync();

    public async Task<ChangeRoleResult> ChangeRoleAsync(int targetUserId, UserRole role, int actingUserId)
    {
        // An admin can never demote themselves, so there is always at least one admin
        if (targetUserId == actingUserId)
            return new ChangeRoleResult(null, "You can't change your own role.", false);

        var user = await _db.Users.FindAsync(targetUserId);
        if (user is null) return new ChangeRoleResult(null, null, true);

        var wasStaff = user.Role != UserRole.Customer;
        user.Role = role;
        await _db.SaveChangesAsync();

        // A demoted agent shouldn't keep owning open tickets
        if (wasStaff && role == UserRole.Customer)
        {
            await _db.Tickets
                .Where(t => t.AssignedAgentId == targetUserId
                         && t.Status != TicketStatus.Resolved
                         && t.Status != TicketStatus.Closed)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.AssignedAgentId, (int?)null));
        }

        return new ChangeRoleResult(
            new UserDto(user.Id, user.FullName, user.Email, user.Role.ToString(), user.CreatedAt),
            null, false);
    }
}