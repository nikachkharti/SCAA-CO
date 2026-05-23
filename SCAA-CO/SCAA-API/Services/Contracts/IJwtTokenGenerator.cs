using SCAA_API.Entities.Authentication;

namespace SCAA_API.Services.Contracts
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(ApplicationUser applicationUser, IEnumerable<string> roles);
    }
}
