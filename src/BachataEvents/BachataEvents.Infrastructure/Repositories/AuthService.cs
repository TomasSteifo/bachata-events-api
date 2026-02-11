using BachataEvents.Application.Abstractions;
using BachataEvents.Application.Auth;
using BachataEvents.Application.Common;
using BachataEvents.Domain.Constants;
using BachataEvents.Domain.Entities;
using BachataEvents.Infrastructure.Auth;
using BachataEvents.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;


namespace BachataEvents.Infrastructure.Repositories;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _db;
    private readonly IJwtTokenGenerator _jwt;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext db,
        IJwtTokenGenerator jwt,
        IOptions<JwtOptions> jwtOptions)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _db = db;
        _jwt = jwt;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        // Ensure roles exist
        await EnsureRoleAsync(AppRoles.User);
        await EnsureRoleAsync(AppRoles.Organizer);

        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            throw new ValidationFailedException(new Dictionary<string, string[]>
            {
                ["email"] = new[] { "Email is already registered." }
            });

        var user = new AppUser
        {
            UserName = request.Email,
            Email = request.Email
        };

        var create = await _userManager.CreateAsync(user, request.Password);
        if (!create.Succeeded)
            throw new ValidationFailedException(new Dictionary<string, string[]>
            {
                ["identity"] = create.Errors.Select(e => e.Description).ToArray()
            });

        var role = request.Role;
        var addRole = await _userManager.AddToRoleAsync(user, role);
        if (!addRole.Succeeded)
            throw new ValidationFailedException(new Dictionary<string, string[]>
            {
                ["role"] = addRole.Errors.Select(e => e.Description).ToArray()
            });

        if (role == AppRoles.Organizer)
        {
            if (request.Organizer is null)
                throw new ValidationFailedException(new Dictionary<string, string[]>
                {
                    ["organizer"] = new[] { "Organizer fields are required." }
                });

            var profile = new OrganizerProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                DisplayName = request.Organizer.DisplayName,
                Website = request.Organizer.Website,
                Instagram = request.Organizer.Instagram
            };

            _db.OrganizerProfiles.Add(profile);
            await _db.SaveChangesAsync(ct);
        }
    }

public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct)
{
    var user = await _userManager.FindByEmailAsync(request.Email);
    if (user is null)
    {
        throw new UnauthorizedException("Invalid credentials.");
    }

    var signInResult = await _signInManager.CheckPasswordSignInAsync(
        user,
        request.Password,
        lockoutOnFailure: true);

    if (!signInResult.Succeeded)
    {
        throw new UnauthorizedException("Invalid credentials.");
    }

    var roles = await _userManager.GetRolesAsync(user);

    // FirstOrDefault is safer than SingleOrDefault because a user can accidentally have multiple roles.
    var role = roles.FirstOrDefault() ?? AppRoles.User;

    Guid? organizerProfileId = null;

    // Only query organizer profile if the user is an organizer.
    if (string.Equals(role, AppRoles.Organizer, StringComparison.Ordinal))
    {
        organizerProfileId = await _db.OrganizerProfiles
            .AsNoTracking()
            .Where(organizerProfile => organizerProfile.UserId == user.Id)
            .Select(organizerProfile => (Guid?)organizerProfile.Id)
            .FirstOrDefaultAsync(ct);
    }

    var token = _jwt.CreateToken(user, role, organizerProfileId, _jwtOptions);

    return new AuthResponse(
        Token: token,
        UserId: user.Id,
        Email: user.Email ?? request.Email,
        Role: role,
        OrganizerProfileId: organizerProfileId
    );
}


public async Task<MeResponse> MeAsync(string userId, CancellationToken ct)
{
    var user = await _userManager.FindByIdAsync(userId);
    if (user is null)
    {
        throw new UnauthorizedException("User not found.");
    }

    var roles = await _userManager.GetRolesAsync(user);
    var role = roles.FirstOrDefault() ?? AppRoles.User;

    Guid? organizerProfileId = null;

    if (string.Equals(role, AppRoles.Organizer, StringComparison.Ordinal))
    {
        organizerProfileId = await _db.OrganizerProfiles
            .AsNoTracking()
            .Where(organizerProfile => organizerProfile.UserId == user.Id)
            .Select(organizerProfile => (Guid?)organizerProfile.Id)
            .FirstOrDefaultAsync(ct);
    }

    return new MeResponse(user.Id, user.Email ?? string.Empty, role, organizerProfileId);
}

private async Task EnsureRoleAsync(string roleName)
{
    var roleExists = await _roleManager.RoleExistsAsync(roleName);
    if (roleExists)
    {
        return;
    }

    await _roleManager.CreateAsync(new IdentityRole(roleName));
}

}
