using Backend.Models;

namespace Backend.Interfaces
{
    public interface ICollectionServices
    {
        Task<bool> CollectionExist(int id);
        Task<ResponseBase> AddCollection(CollectionModel collection);
        Task SaveChangesAsync();
        Task<CollectionModel> GetCollectionById(int id);
        Task<CollectionModel> GetCollectionWithDetailsById(int smart_collection_id);
        Task<int> GetCollectionCount();
        Task<CollectionModel> GetCollectionByIdIncludingRulesAndImages(int smart_collection_id);
        Task<IEnumerable<RulesModel>> GetRulesByCollectionId(int smart_collection_id);
        Task<IEnumerable<CollectionImageModel>> GetImagesByCollectionId(int smart_collection_id);
        Task DeleteRules(IEnumerable<RulesModel> rules);
        Task DeleteImages(IEnumerable<CollectionImageModel> images);
        Task DeleteCollection(CollectionModel collection);
        Task<CollectionModel> GetCollectionWithDetailsAndRelatedById(int smart_collection_id);
        Task<IEnumerable<CollectionModel>> GetAllCollections(
            int page,
            int pageSize,
            string sortBy,
            string sortDirection,
            string filter,
            string search
        );

        Task<IEnumerable<CollectionModel>> GetAllCollection();

        Task<IEnumerable<CollectionModel>> GetMenyCategoryCollections();
        Task<Dictionary<int, CollectionModel>> GetAllCollectionsWithRulesAsDictionary();
        Task<List<CollectionModel>> GetCollectionsByTitle(string title);
        Task<List<CollectionModel>> GetCollectionsByIdList(List<int> collectionIds);
        Task AddRulesToCollection(int smart_collection_id, RulesModel rules);
        Task AddImageToCollection(int smart_collection_id, CollectionImageModel images);
        Task<List<CollectionModel>> GetProductsByCollection(string title);
        Task<IEnumerable<ProductModel>> GetProductsByCollectionTitle(string title);

        Task<(List<ProductModel> Products, int TotalCount)> GetRelatedProductsByRulesAsync(List<string> ruleTitles, int skip, int take);
        Task<(List<ProductModel> Products, int TotalCount)> GetRelatedProductsToCollections(
            int smartCollectionId,
            int skip,
            int take,
            string? productType = null,
            string? productCollection = null,
            string? vendor = null,
            string? availability = null,
            decimal? priceMin = null,
            decimal? priceMax = null,
            string? sort = null
        );
        Task UpsertCollectionProductPositions(int smartCollectionId, List<(long productId, int position)> positions);

    }
}
