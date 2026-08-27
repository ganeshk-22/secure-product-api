using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureProductApi.Application.DTOs;
using SecureProductApi.Application.Interfaces;
using SecureProductApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace SecureProductApi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly LinkGenerator _linkGenerator;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthRepository _authRepository;
        private readonly ITokenService _tokenService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            LinkGenerator linkGenerator,
            IHttpContextAccessor httpContextAccessor,
            IAuthRepository authRepository,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _emailService = emailService;
            _linkGenerator = linkGenerator;
            _httpContextAccessor = httpContextAccessor;
            _authRepository = authRepository;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser is not null)
                return Conflict(new { message = "Email is already registered." });

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            // every new registration defaults to "User" role
            await _userManager.AddToRoleAsync(user, "User");

            // generate email confirmation token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = HtmlEncoder.Default.Encode(Convert.ToBase64String(Encoding.UTF8.GetBytes(token)));

            var confirmationLink = _linkGenerator.GetUriByAction(
                httpContext: _httpContextAccessor.HttpContext!,
                action: "ConfirmEmail",
                controller: "Auth",
                values: new { userId = user.Id, token = encodedToken });

            await _emailService.SendVerificationEmailAsync(user.Email, confirmationLink!);

            return Ok(new RegisterResponse("Registration successful. Please check your email to verify your account."));
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return NotFound(new { message = "Invalid user." });

            string decodedToken;
            try
            {
                decodedToken = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            }
            catch
            {
                return BadRequest(new { message = "Invalid token format." });
            }

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            if (!result.Succeeded)
                return BadRequest(new { message = "Email confirmation failed.", errors = result.Errors.Select(e => e.Description) });

            return Ok(new { message = "Email confirmed successfully. You can now log in." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
                return Unauthorized(new { message = "Invalid email or password." });

            if (!await _userManager.IsEmailConfirmedAsync(user))
                return Unauthorized(new { message = "Please verify your email before logging in." });

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.GenerateAccessToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

            await _authRepository.AddRefreshTokenAsync(refreshToken);
            await _authRepository.SaveChangesAsync();

            return Ok(new AuthResponse(accessToken, refreshToken.Token, DateTime.UtcNow.AddMinutes(15)));
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
        {
            var storedToken = await _authRepository.GetRefreshTokenAsync(request.RefreshToken);

            if (storedToken is null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
                return Unauthorized(new { message = "Invalid or expired refresh token." });

            storedToken.IsRevoked = true;

            var roles = await _userManager.GetRolesAsync(storedToken.User);
            var newAccessToken = _tokenService.GenerateAccessToken(storedToken.User, roles);
            var newRefreshToken = _tokenService.GenerateRefreshToken(storedToken.UserId);

            await _authRepository.AddRefreshTokenAsync(newRefreshToken);
            await _authRepository.SaveChangesAsync();

            return Ok(new AuthResponse(newAccessToken, newRefreshToken.Token, DateTime.UtcNow.AddMinutes(15)));
        }
    }
}