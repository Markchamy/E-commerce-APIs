using System.Security.Claims;

namespace Backend.Auth
{
    public interface IJwtTokenService
    {
        /// <summary>
        /// Builds a signed JWT for the given user. The "store_id" claim is the
        /// hook TenantMiddleware reads to populate ITenantContext.
        /// </summary>
        string CreateToken(long userId, string email, string? role, int storeId, IEnumerable<Claim>? extraClaims = null);
    }
}
