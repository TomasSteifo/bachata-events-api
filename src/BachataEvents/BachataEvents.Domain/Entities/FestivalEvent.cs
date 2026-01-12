namespace BachataEvents.Domain.Entities;

public sealed class FestivalEvent
{
    public Guid Id { get; set; }

    public Guid OrganizerProfileId { get; set; }
    public OrganizerProfile OrganizerProfile { get; set; } = default!;

    public string Title { get; set; } = default!;
    public string? Description { get; set; }

    public string Country { get; set; } = default!;
    public string City { get; set; } = default!;
    public string? VenueName { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public string? WebsiteUrl { get; set; }
    public string? TicketUrl { get; set; }

    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; } // UTC
}
