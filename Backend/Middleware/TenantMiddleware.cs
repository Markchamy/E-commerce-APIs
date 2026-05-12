using System.Security.Claims;
using Backend.Interfaces;

namespace Backend.Middleware
{
    public class TenantMiddleware
    {
        public const string HeaderName = "X-Store-Id";
        public const string JwtClaimName = "store_id";

        // Only the platform-level super_admin role can switch tenants via
        // X-Store-Id. Per-store admins/managers/employees are pinned to their
        // assigned store by the JWT store_id claim.
        public const string SuperAdminRole = "super_admin";

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
                && string.Equals(role, SuperAdminRole, StringComparison.OrdinalIgnoreCase);

            if (isSuperAdmin)
            {
                // super_admin operates outside any tenant by default. They only
                // gain access to a store's data when they explicitly opt in by
                // sending X-Store-Id. Without it, mark the context as tenant-
                // blind so every IStoreScoped query filter returns zero rows —
                // super_admin's job is store lifecycle, not store contents.
                if (headerStoreId.HasValue) tenantContext.SetStore(headerStoreId.Value);
                else tenantContext.SetTenantBlind();
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
