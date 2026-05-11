using Backend.Interfaces;

namespace Backend.Middleware
{
    public class TenantMiddleware
    {
        public const string HeaderName = "X-Store-Id";
        public const string JwtClaimName = "store_id";

        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
        {
            // Priority 1: explicit X-Store-Id header. Used by the CMS admin panel,
            // which sends the currently-selected store on every API call.
            if (context.Request.Headers.TryGetValue(HeaderName, out var headerVal)
                && int.TryParse(headerVal, out var fromHeader)
                && fromHeader > 0)
            {
                tenantContext.SetStore(fromHeader);
            }
            // Priority 2: JWT "store_id" claim. Used by authenticated storefront
            // users whose token already encodes which store they belong to.
            else if (context.User?.Identity?.IsAuthenticated == true)
            {
                var claim = context.User.FindFirst(JwtClaimName)?.Value;
                if (int.TryParse(claim, out var fromClaim) && fromClaim > 0)
                {
                    tenantContext.SetStore(fromClaim);
                }
            }

            // If neither path resolved a tenant, StoreId stays null. Controllers
            // that need tenancy can check ITenantContext.IsResolved and return
            // 400 — but in this phase nothing enforces it yet, so existing
            // routes keep working unchanged.

            await _next(context);
        }
    }
}
