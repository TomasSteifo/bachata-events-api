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
            throw new UnauthorizedException("Invalid credentials.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
            throw new UnauthorizedException("Invalid credentials.");

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.SingleOrDefault() ?? AppRoles.User;

        var organizerProfileId = await _db.OrganizerProfiles
            .Where(x => x.UserId == user.Id)
            .Select(x => (Guid?)x.Id)
            .SingleOrDefaultAsync(ct);

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
            throw new UnauthorizedException("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.SingleOrDefault() ?? AppRoles.User;

        var organizerProfileId = await _db.OrganizerProfiles
            .Where(x => x.UserId == user.Id)
            .Select(x => (Guid?)x.Id)
            .SingleOrDefaultAsync(ct);

        return new MeResponse(user.Id, user.Email ?? "", role, organizerProfileId);
    }

    private async Task EnsureRoleAsync(string roleName)
    {
        if (await _roleManager.RoleExistsAsync(roleName)) return;
        await _roleManager.CreateAsync(new IdentityRole(roleName));
    }
}
