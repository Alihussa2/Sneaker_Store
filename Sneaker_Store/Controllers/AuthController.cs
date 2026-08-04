using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Sneaker_Store.Services;

namespace Sneaker_Store.Controllers;

public record LoginRequest(string Email, string Kode);

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IKundeRepository _kundeRepository;

    public AuthController(IKundeRepository kundeRepository)
    {
        _kundeRepository = kundeRepository;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Kode))
        {
            return BadRequest("Udfyld email og kode.");
        }

        var kunde = _kundeRepository.FindByEmail(request.Email);
        if (kunde is null || !_kundeRepository.VerifyPassword(kunde, request.Kode))
        {
            return Unauthorized("Forkert email eller kode.");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, kunde.KundeId.ToString()),
            new(ClaimTypes.Email, kunde.Email),
            new(ClaimTypes.Name, $"{kunde.Navn} {kunde.Efternavn}"),
            new(ClaimTypes.Role, kunde.IsAdmin ? "Admin" : "Kunde"),
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return Ok(new { kunde.KundeId, kunde.Navn, kunde.Efternavn, kunde.Email, kunde.IsAdmin });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok();
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Unauthorized();
        }

        return Ok(new
        {
            kundeId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            navn = User.FindFirstValue(ClaimTypes.Name),
            email = User.FindFirstValue(ClaimTypes.Email),
            isAdmin = User.IsInRole("Admin"),
        });
    }
}
