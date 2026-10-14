using SupportDesk.Domain;

namespace SupportDesk.Application.Users;

public record UserDto(int Id, string FullName, string Email, string Role, DateTime CreatedAt);

public record ChangeRoleRequest(UserRole Role);

public record ChangeRoleResult(UserDto? User, string? Error, bool NotFound);

public interface IUserAdminService
{
    Task<List<UserDto>> GetAllAsync();
    Task<ChangeRoleResult> ChangeRoleAsync(int targetUserId, UserRole role, int actingUserId);
}