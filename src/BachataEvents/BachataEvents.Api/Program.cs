using System.Text;
using BachataEvents.Application.Abstractions;
using BachataEvents.Application.Mapping;
using BachataEvents.Application.Validation;
using BachataEvents.Domain.Constants;
using BachataEvents.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;
using BachataEvents.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ---- Serilog ----
builder.Host.UseSerilog((ctx, lc) =>
{
    lc.ReadFrom.Configuration(ctx.Configuration)
      .Enrich.FromLogContext()
      .WriteTo.Console();

    var aiConn = ctx.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
    if (!string.IsNullOrWhiteSpace(aiConn))
    {
        lc.WriteTo.ApplicationInsights(aiConn, new TraceTelemetryConverter());
    }
});

// ---- AppInsights (optional, enabled via env var connection string) ----
builder.Services.AddApplicationInsightsTelemetry();

// ---- Controllers + ProblemDetails ----
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<GlobalExceptionMiddleware>();



// Force consistent API behavior (we handle validation ourselves)
builder.Services.Configure<ApiBehaviorOptions>(opt =>
{
    opt.SuppressModelStateInvalidFilter = true;
});

// ---- AutoMapper ----
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<FestivalMappingProfile>();
});

// ---- FluentValidation ----
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

// ---- Infrastructure ----
builder.Services.AddInfrastructure(builder.Configuration);

// ---- JWT Auth ----
var issuer = builder.Configuration["JWT_ISSUER"] ?? "";
var audience = builder.Configuration["JWT_AUDIENCE"] ?? "";
var signingKey = builder.Configuration["JWT_SIGNING_KEY"] ?? "";

if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience) || string.IsNullOrWhiteSpace(signingKey))
{
    throw new InvalidOperationException("JWT env vars missing: JWT_ISSUER, JWT_AUDIENCE, JWT_SIGNING_KEY.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("OrganizerOnly", p => p.RequireRole(AppRoles.Organizer));
});

var app = builder.Build();

// ---- Middleware ----
app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Required for WebApplicationFactory in integration tests
public partial class Program { }
