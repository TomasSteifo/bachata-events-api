using BachataEvents.Application.Abstractions;
using BachataEvents.Infrastructure.Auth;
using BachataEvents.Infrastructure.Persistence;
using BachataEvents.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BachataEvents.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config["DB_CONNECTION_STRING"];
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("DB_CONNECTION_STRING is missing (use environment variables / Azure App Settings).");

        services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(connectionString));

        services
            .AddIdentity<AppUser, IdentityRole>(opt =>
            {
                opt.Password.RequireDigit = true;
                opt.Password.RequireLowercase = true;
                opt.Password.RequireUppercase = false;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<JwtOptions>(opt =>
        {
            opt.Issuer = config["JWT_ISSUER"] ?? throw new InvalidOperationException("JWT_ISSUER missing.");
            opt.Audience = config["JWT_AUDIENCE"] ?? throw new InvalidOperationException("JWT_AUDIENCE missing.");
            opt.SigningKey = config["JWT_SIGNING_KEY"] ?? throw new InvalidOperationException("JWT_SIGNING_KEY missing.");
            opt.ExpMinutes = int.TryParse(config["JWT_EXP_MINUTES"], out var m) ? m : 60;
        });

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOrganizerService, OrganizerService>();
        services.AddScoped<IFestivalService, FestivalService>();

        return services;
    }
}
