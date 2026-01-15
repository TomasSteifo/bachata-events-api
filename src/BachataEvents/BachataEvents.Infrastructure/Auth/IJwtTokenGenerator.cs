using BachataEvents.Infrastructure.Auth;

namespace BachataEvents.Infrastructure.Auth;

public interface IJwtTokenGenerator
{
    string CreateToken(AppUser user, string role, Guid? organizerProfileId, JwtOptions options);
}
