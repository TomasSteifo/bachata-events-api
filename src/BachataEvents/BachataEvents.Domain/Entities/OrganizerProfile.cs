namespace BachataEvents.Domain.Entities;

public sealed class OrganizerProfile
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = default!; // FK to Identity user
    public string DisplayName { get; set; } = default!;
    public string? Website { get; set; }
    public string? Instagram { get; set; }

    public List<FestivalEvent> Festivals { get; set; } = new();
}
