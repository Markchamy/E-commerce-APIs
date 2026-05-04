using Backend.Models;

namespace Backend.Interfaces
{
    public interface IInventoryServices
    {
        Task<IEnumerable<InventorySortByModel>> GetAllSortingAsync();
        Task<IEnumerable<ProductModel>> GetAllInventoryWithDetails(
                int page = 1,
                int pageSize = 50,
                string sortBy = "Title",
                string sortDirection = "asc",
                string filter = "",
                string search = ""
            );
        Task<UpdateInventoryBySkuResponse> UpdateInventoryBySkuAsync(UpdateInventoryBySkuDTO request);
    }
}
