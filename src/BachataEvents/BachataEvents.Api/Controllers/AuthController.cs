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

        var result = await _authService.LoginAsync(request, cancellationToken);
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
