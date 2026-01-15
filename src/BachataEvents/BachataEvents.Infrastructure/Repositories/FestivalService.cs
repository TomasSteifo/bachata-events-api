using AutoMapper;
using AutoMapper.QueryableExtensions;
using BachataEvents.Application.Abstractions;
using BachataEvents.Application.Common;
using BachataEvents.Application.Festivals;
using BachataEvents.Domain.Entities;
using BachataEvents.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using BachataEvents.Application.Validation;

namespace BachataEvents.Infrastructure.Repositories;

public sealed class FestivalService : IFestivalService
{
    private readonly AppDbContext _db;
    private readonly IOrganizerService _organizers;
    private readonly IMapper _mapper;
    private readonly IValidator<FestivalQuery> _queryValidator;

    public FestivalService(
        AppDbContext db,
        IOrganizerService organizers,
        IMapper mapper,
        IValidator<FestivalQuery> queryValidator)
    {
        _db = db;
        _organizers = organizers;
        _mapper = mapper;
        _queryValidator = queryValidator;
    }

    public async Task<PagedResult<FestivalListItemDto>> GetPublishedAsync(FestivalQuery query, CancellationToken ct)
    {
        await _queryValidator.ValidateOrThrowAsync(query, ct);

        var (page, pageSize) = Pagination.Normalize(query.Page, query.PageSize);

        IQueryable<FestivalEvent> q = _db.FestivalEvents.AsNoTracking().Where(x => x.IsPublished);

        if (!string.IsNullOrWhiteSpace(query.Country))
        {
            var country = query.Country.Trim();
            q = q.Where(x => x.Country == country);
        }

        if (query.StartDate.HasValue && query.EndDate.HasValue)
        {
            var start = query.StartDate.Value;
            var end = query.EndDate.Value;

            // IMPORTANT: Fully inside range
            q = q.Where(x => x.StartDate >= start && x.EndDate <= end);
        }
        else if (query.StartDate.HasValue)
        {
            var start = query.StartDate.Value;
            q = q.Where(x => x.StartDate >= start);
        }
        else if (query.EndDate.HasValue)
        {
            var end = query.EndDate.Value;
            q = q.Where(x => x.EndDate <= end);
        }

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var term = query.Q.Trim();
            q = q.Where(x => x.Title.Contains(term) || x.City.Contains(term));
        }

        q = query.SortBy switch
        {
            FestivalSortBy.StartDateDesc => q.OrderByDescending(x => x.StartDate).ThenBy(x => x.Title),
            _ => q.OrderBy(x => x.StartDate).ThenBy(x => x.Title)
        };

        var total = await q.CountAsync(ct);

        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<FestivalListItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return new PagedResult<FestivalListItemDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<FestivalDetailsDto?> GetPublishedByIdAsync(Guid id, CancellationToken ct)
    {
        return await _db.FestivalEvents.AsNoTracking()
            .Where(x => x.Id == id && x.IsPublished)
            .ProjectTo<FestivalDetailsDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(ct);
    }

    public async Task<FestivalDetailsDto> CreateAsync(string userId, CreateFestivalRequest request, CancellationToken ct)
    {
        var organizerId = await _organizers.GetOrganizerProfileIdOrThrowAsync(userId, ct);

        var entity = _mapper.Map<FestivalEvent>(request);
        entity.Id = Guid.NewGuid();
        entity.OrganizerProfileId = organizerId;
        entity.IsPublished = false;
        entity.CreatedAt = DateTime.UtcNow;

        _db.FestivalEvents.Add(entity);
        await _db.SaveChangesAsync(ct);

        return _mapper.Map<FestivalDetailsDto>(entity);
    }

    public async Task<FestivalDetailsDto> UpdateAsync(string userId, Guid id, UpdateFestivalRequest request, CancellationToken ct)
    {
        var organizerId = await _organizers.GetOrganizerProfileIdOrThrowAsync(userId, ct);

        var entity = await _db.FestivalEvents.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) throw new NotFoundException("Festival not found.");

        if (entity.OrganizerProfileId != organizerId)
            throw new ForbiddenException("You do not own this festival.");

        _mapper.Map(request, entity);

        await _db.SaveChangesAsync(ct);
        return _mapper.Map<FestivalDetailsDto>(entity);
    }

    public async Task DeleteAsync(string userId, Guid id, CancellationToken ct)
    {
        var organizerId = await _organizers.GetOrganizerProfileIdOrThrowAsync(userId, ct);

        var entity = await _db.FestivalEvents.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) throw new NotFoundException("Festival not found.");

        if (entity.OrganizerProfileId != organizerId)
            throw new ForbiddenException("You do not own this festival.");

        _db.FestivalEvents.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<FestivalDetailsDto> SetPublishAsync(string userId, Guid id, bool isPublished, CancellationToken ct)
    {
        var organizerId = await _organizers.GetOrganizerProfileIdOrThrowAsync(userId, ct);

        var entity = await _db.FestivalEvents.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) throw new NotFoundException("Festival not found.");

        if (entity.OrganizerProfileId != organizerId)
            throw new ForbiddenException("You do not own this festival.");

        entity.IsPublished = isPublished;
        await _db.SaveChangesAsync(ct);

        return _mapper.Map<FestivalDetailsDto>(entity);
    }

    public async Task<IReadOnlyList<FestivalListItemDto>> GetMineAsync(string userId, CancellationToken ct)
    {
        var organizerId = await _organizers.GetOrganizerProfileIdOrThrowAsync(userId, ct);

        return await _db.FestivalEvents.AsNoTracking()
            .Where(x => x.OrganizerProfileId == organizerId)
            .OrderByDescending(x => x.CreatedAt)
            .ProjectTo<FestivalListItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }
}
