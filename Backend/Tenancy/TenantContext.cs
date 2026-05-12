using Backend.Interfaces;

namespace Backend.Tenancy
{
    public class TenantContext : ITenantContext
    {
        public int? StoreId { get; private set; }
        public bool IsResolved => StoreId.HasValue;
        public bool IsTenantBlind { get; private set; }

        public void SetStore(int storeId)
        {
            if (storeId <= 0)
                throw new ArgumentOutOfRangeException(nameof(storeId), "StoreId must be positive.");
            StoreId = storeId;
            // A resolved store automatically clears the blind flag.
            IsTenantBlind = false;
        }

        public void SetTenantBlind()
        {
            IsTenantBlind = true;
            StoreId = null;
        }
    }
}
