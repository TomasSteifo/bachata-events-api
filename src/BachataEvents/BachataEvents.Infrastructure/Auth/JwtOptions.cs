namespace BachataEvents.Infrastructure.Auth;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public string SigningKey { get; set; } = default!;
    public int ExpMinutes { get; set; } = 60;
}
