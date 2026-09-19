using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Api.Common;
using StudyPlanner.Infrastructure.Auth;

namespace StudyPlanner.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(UserManager<ApplicationUser> userManager, TokenService tokenService) : ControllerBase
{
    public record RegisterRequest(string Email, string Password, string Name);
    public record LoginRequest(string Email, string Password);
    public record AuthResponse(string Token, Guid UserId, string Name, string Email);

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var existing = await userManager.FindByEmailAsync(request.Email);
        if (existing is not null) return Conflict(new { message = "Já existe uma conta com esse e-mail." });

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            Name = request.Name
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new { message = string.Join(" ", result.Errors.Select(e => e.Description)) });
        }

        var token = tokenService.GenerateToken(user);
        return Ok(new AuthResponse(token, user.Id, user.Name, user.Email!));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized(new { message = "E-mail ou senha inválidos." });
        }

        var token = tokenService.GenerateToken(user);
        return Ok(new AuthResponse(token, user.Id, user.Name, user.Email!));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = await userManager.FindByIdAsync(User.GetUserId().ToString());
        if (user is null) return NotFound();

        return Ok(new AuthResponse(string.Empty, user.Id, user.Name, user.Email!));
    }
}
