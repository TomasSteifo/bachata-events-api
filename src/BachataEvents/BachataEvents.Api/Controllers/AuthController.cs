using System.Security.Claims;
using BachataEvents.Application.Abstractions;
using BachataEvents.Application.Auth;
using BachataEvents.Application.Validation;
using BachataEvents.Domain.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BachataEvents.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthController(
        IAuthService auth,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _auth = auth;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        await _registerValidator.ValidateOrThrowAsync(request, ct);
        await _auth.RegisterAsync(request, ct);
        return NoContent();
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        await _loginValidator.ValidateOrThrowAsync(request, ct);
        var result = await _auth.LoginAsync(request, ct);
        return Ok(result);
    }

[Authorize]
[HttpGet("me")]
public ActionResult<MeResponse> Me()
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrWhiteSpace(userId))
    {
        return Unauthorized();
    }

    var email = User.FindFirstValue(ClaimTypes.Email);
    var role = User.FindFirstValue(ClaimTypes.Role);

    // This matches the custom claim created in JwtTokenGenerator
    var organizerProfileIdClaim = User.FindFirstValue("organizerProfileId");

    Guid? organizerProfileId = null;
    if (!string.IsNullOrWhiteSpace(organizerProfileIdClaim) &&
        Guid.TryParse(organizerProfileIdClaim, out var parsedId))
    {
        organizerProfileId = parsedId;
    }

    var response = new MeResponse(
        userId,
        email ?? string.Empty,
        role ?? AppRoles.User,
        organizerProfileId
    );

    return Ok(response);
}

}
