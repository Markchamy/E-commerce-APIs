namespace Backend.Interfaces
{
    public interface ITenantContext
    {
        int? StoreId { get; }
        bool IsResolved { get; }
        void SetStore(int storeId);
    }
}
