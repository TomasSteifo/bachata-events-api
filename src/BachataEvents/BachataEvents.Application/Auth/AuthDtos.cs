namespace BachataEvents.Application.Auth;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string Role,
    OrganizerFields? Organizer
);

public sealed record OrganizerFields(
    string DisplayName,
    string? Website,
    string? Instagram
);

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthResponse(
    string Token,
    string UserId,
    string Email,
    string Role,
    Guid? OrganizerProfileId
);

public sealed record MeResponse(
    string UserId,
    string Email,
    string Role,
    Guid? OrganizerProfileId
);
