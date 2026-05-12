namespace Backend.Interfaces
{
    public interface ITenantContext
    {
        int? StoreId { get; }
        bool IsResolved { get; }

        /// <summary>
        /// True when the caller is authenticated but operates outside any
        /// tenant (super_admin who hasn't picked a store). Every IStoreScoped
        /// query then returns zero rows — super_admin cannot read products,
        /// customers, orders, inventory or any other store-scoped data.
        /// </summary>
        bool IsTenantBlind { get; }

        void SetStore(int storeId);
        void SetTenantBlind();
    }
}
