
using TestWebApi.Shared.Constructs;

namespace TestWebApi.Shared.Services
{
    public interface ICategoryService
    {
        // Basic CRUD Operations
        Task<CategoryResponse?> GetCategoryByIdAsync(Guid id);
        Task<CategoryDetailsResponse?> GetCategoryDetailsAsync(Guid id);
        Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync();
        Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request);
        Task<CategoryResponse?> UpdateCategoryAsync(UpdateCategoryRequest request);
        Task<bool> DeleteCategoryAsync(Guid id);

        // Advanced Operations
        Task<bool> CategoryExistsAsync(Guid id);
        Task<CategoryResponse?> GetCategoryByNameAsync(string name);
        Task<IEnumerable<CategoryResponse>> SearchCategoriesAsync(string searchTerm);
        Task<IEnumerable<CategoryResponse>> GetCategoriesWithProductsAsync();

        // Pagination & Filtering
        Task<PagedResponse<CategoryResponse>> GetCategoriesPagedAsync(int page, int pageSize);


        Task<int> GetCategoryCountAsync();

        // Business Logic Operations
        Task<IEnumerable<CategorySummaryResponse>> GetCategoriesWithProductCountAsync();
        Task<IEnumerable<CategoryResponse>> GetTopCategoriesByProductCountAsync(int count);
        Task<bool> CanDeleteCategoryAsync(Guid id); // Check if category has products

       
        // Validation
        Task<ValidationResult> ValidateCategoryAsync(CreateCategoryRequest request);
        Task<ValidationResult> ValidateCategoryUpdateAsync(UpdateCategoryRequest request);
    }
}