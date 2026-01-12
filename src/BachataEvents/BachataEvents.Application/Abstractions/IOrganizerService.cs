namespace BachataEvents.Application.Abstractions;

public interface IOrganizerService
{
    Task<Guid> GetOrganizerProfileIdOrThrowAsync(string userId, CancellationToken ct);
    Task<Guid?> GetOrganizerProfileIdAsync(string userId, CancellationToken ct);
}
