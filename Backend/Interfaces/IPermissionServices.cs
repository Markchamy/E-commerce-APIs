using Backend.Models;

namespace Backend.Interfaces
{
    public interface IPermissionServices
    {
        Task<IEnumerable<PermissionsModel>> GetAllPermissionsAsync();
    }
}
