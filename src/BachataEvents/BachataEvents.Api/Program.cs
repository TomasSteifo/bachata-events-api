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
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

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
builder.Services.AddControllers()
.AddJsonOptions(jsonOptions =>
    {
        // Frontend sends camelCase JSON: { "email": "...", "password": "..." }
        // This makes the API accept camelCase, and also be tolerant to casing.
        jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        jsonOptions.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Add JWT Bearer authentication support to Swagger UI (shows the "Authorize" button)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    // Make Swagger send the token automatically to secured endpoints
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddTransient<GlobalExceptionMiddleware>();



builder.Services.Configure<ApiBehaviorOptions>(apiBehaviorOptions =>
{
    apiBehaviorOptions.SuppressModelStateInvalidFilter = false;
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

// ---- CORS ----
// Allow the Vite development server to call the API during local development
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCorsPolicy", corsPolicyBuilder =>
    {
        corsPolicyBuilder
            .WithOrigins("http://localhost:8080","https://orange-dune-0602a7903.2.azurestaticapps.net")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


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

// ---- CORS ----
app.UseCors("FrontendCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Required for WebApplicationFactory in integration tests
public partial class Program { }
