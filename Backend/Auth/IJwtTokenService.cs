using System.Security.Claims;

namespace Backend.Auth
{
    public record RefreshTokenPair(string Token, string Hash);

    public interface IJwtTokenService
    {
        /// <summary>
        /// Builds a signed JWT for the given user. The "store_id" claim is the
        /// hook TenantMiddleware reads to populate ITenantContext.
        /// </summary>
        string CreateToken(long userId, string email, string? role, int storeId, IEnumerable<Claim>? extraClaims = null);

        /// <summary>
        /// Generates a cryptographically random refresh token and its SHA-256
        /// hex digest. Persist the digest, return the raw token to the client.
        /// </summary>
        RefreshTokenPair CreateRefreshToken();

        /// <summary>SHA-256 hex digest of an arbitrary string.</summary>
        string HashToken(string token);
    }
}
