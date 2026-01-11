namespace BachataEvents.Application.Festivals;

public enum FestivalSortBy
{
    StartDateAsc = 0,
    StartDateDesc = 1
}

public sealed record FestivalListItemDto(
    Guid Id,
    string Title,
    string Country,
    string City,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsPublished
);

public sealed record FestivalDetailsDto(
    Guid Id,
    Guid OrganizerProfileId,
    string Title,
    string? Description,
    string Country,
    string City,
    string? VenueName,
    DateOnly StartDate,
    DateOnly EndDate,
    string? WebsiteUrl,
    string? TicketUrl,
    bool IsPublished,
    DateTime CreatedAt
);

public sealed record CreateFestivalRequest(
    string Title,
    string? Description,
    string Country,
    string City,
    string? VenueName,
    DateOnly StartDate,
    DateOnly EndDate,
    string? WebsiteUrl,
    string? TicketUrl
);

public sealed record UpdateFestivalRequest(
    string Title,
    string? Description,
    string Country,
    string City,
    string? VenueName,
    DateOnly StartDate,
    DateOnly EndDate,
    string? WebsiteUrl,
    string? TicketUrl
);

public sealed record PublishFestivalRequest(bool IsPublished);

public sealed record FestivalQuery(
    string? Country,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? Q,
    FestivalSortBy SortBy,
    int? Page,
    int? PageSize
);
