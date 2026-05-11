using System.Security.Claims;
using Backend.Interfaces;

namespace Backend.Middleware
{
    public class TenantMiddleware
    {
        public const string HeaderName = "X-Store-Id";
        public const string JwtClaimName = "store_id";

        // Roles allowed to switch tenants via X-Store-Id. Anyone else is locked
        // to whatever store_id their JWT was issued with.
        private static readonly string[] SuperAdminRoles = { "admin", "manager" };

        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
        {
            var user = context.User;
            var isAuthenticated = user?.Identity?.IsAuthenticated == true;

            int? jwtStoreId = null;
            if (isAuthenticated)
            {
                var claim = user!.FindFirst(JwtClaimName)?.Value;
                if (int.TryParse(claim, out var fromClaim) && fromClaim > 0) jwtStoreId = fromClaim;
            }

            int? headerStoreId = null;
            if (context.Request.Headers.TryGetValue(HeaderName, out var headerVal)
                && int.TryParse(headerVal, out var fromHeader)
                && fromHeader > 0)
            {
                headerStoreId = fromHeader;
            }

            var role = user?.FindFirst(ClaimTypes.Role)?.Value;
            var isSuperAdmin = isAuthenticated
                && role != null
                && SuperAdminRoles.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));

            if (isSuperAdmin)
            {
                // Header wins; JWT claim is a fallback default.
                var resolved = headerStoreId ?? jwtStoreId;
                if (resolved.HasValue) tenantContext.SetStore(resolved.Value);
            }
            else if (isAuthenticated)
            {
                // Non-admin authenticated users: JWT claim is law. A header is
                // tolerated only when it matches the claim; any mismatch is a
                // tenant-isolation attempt and we reject it outright.
                if (headerStoreId.HasValue && jwtStoreId.HasValue && headerStoreId.Value != jwtStoreId.Value)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Forbidden: X-Store-Id does not match the token's store_id.");
                    return;
                }
                if (jwtStoreId.HasValue) tenantContext.SetStore(jwtStoreId.Value);
            }
            else
            {
                // Unauthenticated routes (login, public storefront pages): header
                // is the only available signal.
                if (headerStoreId.HasValue) tenantContext.SetStore(headerStoreId.Value);
            }

            await _next(context);
        }
    }
}
