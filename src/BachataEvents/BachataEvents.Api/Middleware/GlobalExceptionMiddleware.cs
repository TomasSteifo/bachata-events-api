using System.Diagnostics;
using BachataEvents.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace BachataEvents.Api.Middleware;

public sealed class GlobalExceptionMiddleware : IMiddleware
{
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(ILogger<GlobalExceptionMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

            _logger.LogError(ex, "Unhandled exception. TraceId={TraceId}", traceId);

            var (status, title, extensions) = ex switch
            {
                ValidationFailedException v => (StatusCodes.Status400BadRequest, "Validation failed", new Dictionary<string, object?>
                {
                    ["errors"] = v.Errors
                }),
                UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized", null),
                ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden", null),
                NotFoundException => (StatusCodes.Status404NotFound, "Not Found", null),
                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", null)
            };

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = status == 500 ? "An unexpected error occurred." : ex.Message,
                Instance = context.Request.Path
            };

            problem.Extensions["traceId"] = traceId;
            if (extensions is not null)
            {
                foreach (var kv in extensions)
                    problem.Extensions[kv.Key] = kv.Value;
            }

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
