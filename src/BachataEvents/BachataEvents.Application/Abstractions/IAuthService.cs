using BachataEvents.Application.Auth;

namespace BachataEvents.Application.Abstractions;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequest request, CancellationToken ct);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct);
    Task<MeResponse> MeAsync(string userId, CancellationToken ct);
}
