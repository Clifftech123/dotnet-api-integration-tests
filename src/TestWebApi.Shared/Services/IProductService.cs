
using TestWebApi.Shared.Constructs;

namespace TestWebApi.Shared.Services
{
    public interface IProductService
    {
        // Basic CRUD Operations
        Task<ProductResponse?> GetProductByIdAsync(Guid id);
        Task<ProductDetailsResponse?> GetProductDetailsAsync(Guid id);
        Task<IEnumerable<ProductResponse>> GetAllProductsAsync();
        Task<ProductResponse> CreateProductAsync(CreateProductRequest request);
        Task<ProductResponse?> UpdateProductAsync(UpdateProductRequest request);
        Task<bool> DeleteProductAsync(Guid id);

        // Advanced Operations
        Task<bool> ProductExistsAsync(Guid id);
        Task<IEnumerable<ProductResponse>> GetProductsByCategoryAsync(Guid categoryId);
        Task<IEnumerable<ProductResponse>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<IEnumerable<ProductResponse>> SearchProductsAsync(string searchTerm);

        // Pagination & Filtering
        Task<PagedResponse<ProductResponse>> GetProductsPagedAsync(int page, int pageSize);      
        Task<int> GetProductCountByCategoryAsync(Guid categoryId);

        // Validation
        Task<ValidationResult> ValidateProductAsync(CreateProductRequest request);
        Task<ValidationResult> ValidateProductUpdateAsync(UpdateProductRequest request);
    }
}