using BachataEvents.Application.Common;
using BachataEvents.Application.Festivals;

namespace BachataEvents.Application.Abstractions;

public interface IFestivalService
{
    Task<PagedResult<FestivalListItemDto>> GetPublishedAsync(FestivalQuery query, CancellationToken ct);
    Task<FestivalDetailsDto?> GetPublishedByIdAsync(Guid id, CancellationToken ct);

    Task<FestivalDetailsDto> CreateAsync(string userId, CreateFestivalRequest request, CancellationToken ct);
    Task<FestivalDetailsDto> UpdateAsync(string userId, Guid id, UpdateFestivalRequest request, CancellationToken ct);
    Task DeleteAsync(string userId, Guid id, CancellationToken ct);
    Task<FestivalDetailsDto> SetPublishAsync(string userId, Guid id, bool isPublished, CancellationToken ct);

    Task<IReadOnlyList<FestivalListItemDto>> GetMineAsync(string userId, CancellationToken ct);
}
