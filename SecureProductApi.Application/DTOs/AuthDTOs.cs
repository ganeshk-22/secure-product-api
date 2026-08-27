using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureProductApi.Application.DTOs
{
    public record RegisterRequest(string Email, string Password, string FullName);

    public record RegisterResponse(string Message);

    public record LoginRequest(string Email, string Password);

    public record AuthResponse(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAt);

    public record RefreshTokenRequest(string RefreshToken);
}
