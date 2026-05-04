using Backend.Models;

namespace Backend.Interfaces
{
    public interface ISearchServices
    {
        Task<GlobalSearchResponse> GlobalSearchAsync(GlobalSearchRequest request);
    }
}
