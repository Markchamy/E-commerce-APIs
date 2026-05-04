using Backend.Models;

namespace Backend.Interfaces
{
    public interface IPurchaseOrderServices
    {
        Task<bool> SupplierExists(string email);
        Task<ResponseBase> AddSupplier(SupplierDTO supplierDTO);
        Task<ResponseBase> GetAllSuppliers();
        Task<ResponseBase> UpdateSupplier(int id, SupplierDTO supplierDTO);
    }
}
