using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportDesk.Application.Auth
{
    public record RegisterRequest(
      [Required, MaxLength(150)] string FullName,
      [Required, EmailAddress] string Email,
      [Required, MinLength(8)] string Password);

    public record LoginRequest(
        [Required, EmailAddress] string Email,
        [Required] string Password);

    public record AuthResponse(string Token, int UserId, string FullName, string Email, string Role);
}
