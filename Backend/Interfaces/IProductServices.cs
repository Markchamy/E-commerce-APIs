using Backend.Models;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Backend.Interfaces
{
    public interface IProductServices
{
        Task<bool> ProductExists(long Id);
        Task<bool> ProductTitleExists(string title);
        Task<ResponseBase> AddProduct(ProductModel product);
        Task<ProductModel> GetProductById(long id);
        Task DeleteProduct(ProductModel product);
        Task SaveChangesAsync();
        Task<List<ProductModel>> GetProductsWhereNewIsTrue();
        Task<IEnumerable<VariantModel>> GetVariantsByProductId(long productId);
        Task<IEnumerable<Options>> GetOptionsByProductId(long productId);
        Task DeleteVariants(IEnumerable<VariantModel> variants);
        Task DeleteOptions(IEnumerable<Options> options);
        Task DeleteImages(IEnumerable<ProductImages> images);
        Task<int> GetProductsCount();
        Task<ProductModel> GetProductWithDetailsById(long id);
        Task<IEnumerable<ProductModel>> GetAllProducts();
        Task<IEnumerable<ProductModel>> GetAllProductsWithDetails(
                int page = 1,
                int pageSize = 50,
                string sortBy = "Title",
                string sortDirection = "asc",
                string filter = "",
                string search = ""
            );
        Task<ProductModel> GetProductByIdIncludingVariantsAndOptions(long productId);
        Task AddImageToProduct(long productId,long variantId, ProductImages image);
        Task AddImage(long productId, ProductImages image);
        Task<List<ProductImages>> GetImagesByProductId(long productId);
        Task<ProductImages> GetImageByProductIdAndImageId(long productId, long imageId);
        Task<int> GetImagesCountByProductId(long productId);
        Task<bool> UpdateImage(long productId, long imageId, ProductImagesDTO updatedImage);
        Task<bool> UpdateImagePosition(long productId, long imageId, int position);
        Task<bool> DeleteImage(long productId, long imageId);
        Task AddVariantToProduct(long productId, VariantModel variant);
        Task<List<VariantModel>> GetVariantByProductId(long productId);
        Task<VariantModel> GetVariantByProductIdAndVariantId(long productId, long variantId);
        Task<int> GetVariantsCountByProductId(long productId);
        Task<bool> UpdateVariant(long productId, long variantId ,VariantDTO variantDTO);
        Task<bool> DeleteVariant(long productId, long variantId);
        Task<bool> UpdateVariantPositions(long productId, List<VariantPositionUpdateDTO> variantPositions);
        Task<CollectionModel> GetCollectionByTitle(string product_collection);
        Task ImportProductsAsync(IEnumerable<ProductModel> products);
        Task<IEnumerable<ProductModel>> GetProductsByIds(IEnumerable<long> productIds);
        Task<IEnumerable<ProductModel>> SearchProductsAsync(string query);
        Task ImportShopifyProductsAsync(List<CsvProductModel> products);
        Task<ProductModel> GetProductsByTitleAsync(string title);
        Task<IEnumerable<ProductModel>> GetProductsWithSkuAsync(int page = 1, int limit = 250);
    }
}
