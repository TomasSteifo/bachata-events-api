using BachataEvents.Application.Abstractions;
using BachataEvents.Application.Common;
using BachataEvents.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BachataEvents.Infrastructure.Repositories;

public sealed class OrganizerService : IOrganizerService
{
    private readonly AppDbContext _db;

    public OrganizerService(AppDbContext db) => _db = db;

    public async Task<Guid> GetOrganizerProfileIdOrThrowAsync(string userId, CancellationToken ct)
    {
        var id = await _db.OrganizerProfiles
            .Where(x => x.UserId == userId)
            .Select(x => (Guid?)x.Id)
            .SingleOrDefaultAsync(ct);

        if (!id.HasValue)
            throw new ForbiddenException("Organizer profile not found for user.");

        return id.Value;
    }

    public async Task<Guid?> GetOrganizerProfileIdAsync(string userId, CancellationToken ct)
    {
        return await _db.OrganizerProfiles
            .Where(x => x.UserId == userId)
            .Select(x => (Guid?)x.Id)
            .SingleOrDefaultAsync(ct);
    }
}
