using System.Security.Claims;
using BachataEvents.Application.Abstractions;
using BachataEvents.Application.Auth;
using BachataEvents.Application.Validation;
using BachataEvents.Domain.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BachataEvents.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<RegisterRequest> _registerRequestValidator;
    private readonly IValidator<LoginRequest> _loginRequestValidator;

    public AuthController(
        IAuthService authService,
        IValidator<RegisterRequest> registerRequestValidator,
        IValidator<LoginRequest> loginRequestValidator)
    {
        _authService = authService;
        _registerRequestValidator = registerRequestValidator;
        _loginRequestValidator = loginRequestValidator;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest? request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid request body",
                Detail = "Request body was empty or could not be parsed as JSON.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        await _registerRequestValidator.ValidateOrThrowAsync(request, cancellationToken);
        await _authService.RegisterAsync(request, cancellationToken);

        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest? request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid request body",
                Detail = "Request body was empty or could not be parsed as JSON.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        await _loginRequestValidator.ValidateOrThrowAsync(request, cancellationToken);

        AuthResponse result = await _authService.LoginAsync(request, cancellationToken);
        return Ok(result);
    }

    // Force JWT so the API never tries cookie/Identity redirects (Account/Login)
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("me")]
    public ActionResult<MeResponse> Me()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        string? email = User.FindFirstValue(ClaimTypes.Email);
        string? role = User.FindFirstValue(ClaimTypes.Role);

        string? organizerProfileIdClaim = User.FindFirstValue("organizerProfileId");

        Guid? organizerProfileId = null;
        if (!string.IsNullOrWhiteSpace(organizerProfileIdClaim) &&
            Guid.TryParse(organizerProfileIdClaim, out Guid parsedId))
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
