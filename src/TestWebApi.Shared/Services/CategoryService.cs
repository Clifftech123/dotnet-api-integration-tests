using TestWebApi.Core.Entities;
using TestWebApi.Shared.Constructs;
using TestWebApi.Shared.Extensions;
using TestWebApi.Shared.Repositories;

namespace TestWebApi.Shared.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IGenericRepository<Category> _categoryRepository;
        private readonly IGenericRepository<Product> _productRepository;

        public CategoryService(IGenericRepository<Category> categoryRepository, IGenericRepository<Product> productRepository)
        {
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        }

        #region Basic CRUD Operations

        public async Task<CategoryResponse?> GetCategoryByIdAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            return category?.ToCategoryResponse();
        }

        public async Task<CategoryDetailsResponse?> GetCategoryDetailsAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return null;

            var products = await _productRepository.FindAsync(p => p.CategoryId == id);
            return category.ToCategoryDetailsResponse(products);
        }

        public async Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return categories.ToCategoryResponseList();
        }

        public async Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request)
        {
            // Validate category using the validation method
            var validationResult = await ValidateCategoryAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => $"{e.Field}: {e.Message}"));
                throw new ArgumentException($"Category validation failed: {errors}");
            }

            var category = request.ToCategory();
            var createdCategory = await _categoryRepository.CreateAsync(category);
            return createdCategory.ToCategoryResponse();
        }

        public async Task<CategoryResponse?> UpdateCategoryAsync(UpdateCategoryRequest request)
        {
            var validationResult = await ValidateCategoryUpdateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => $"{e.Field}: {e.Message}"));
                throw new ArgumentException($"Category validation failed: {errors}");
            }

            var existingCategory = await _categoryRepository.GetByIdAsync(request.Id);
            if (existingCategory == null)
                return null;

            existingCategory.UpdateFromRequest(request);
            await _categoryRepository.UpdateAsync(existingCategory);
            return existingCategory.ToCategoryResponse();
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            // Check if category can be deleted (no associated products)
            if (!await CanDeleteCategoryAsync(id))
                return false;

            return await _categoryRepository.DeleteAsync(id);
        }

        #endregion

        #region Advanced Operations

        public async Task<bool> CategoryExistsAsync(Guid id)
        {
            return await _categoryRepository.ExistsAsync(id);
        }

        public async Task<CategoryResponse?> GetCategoryByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var category = await _categoryRepository.GetFirstOrDefaultAsync(c => c.Name == name);
            return category?.ToCategoryResponse();
        }

        public async Task<IEnumerable<CategoryResponse>> SearchCategoriesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllCategoriesAsync();

            var categories = await _categoryRepository.FindAsync(
                c => c.Name.Contains(searchTerm) || c.Description.Contains(searchTerm));
            return categories.ToCategoryResponseList();
        }

        public async Task<IEnumerable<CategoryResponse>> GetCategoriesWithProductsAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var categoriesWithProducts = new List<CategoryResponse>();

            foreach (var category in categories)
            {
                var hasProducts = await _productRepository.CountAsync(p => p.CategoryId == category.Id) > 0;
                if (hasProducts)
                {
                    categoriesWithProducts.Add(category.ToCategoryResponse());
                }
            }

            return categoriesWithProducts;
        }

        public async Task<bool> CanDeleteCategoryAsync(Guid id)
        {
            // Check if products are associated with this category then we cannot delete it
            var productCount = await _productRepository.CountAsync(p => p.CategoryId == id);
            return productCount == 0;
        }

        #endregion

        #region Pagination & Filtering

        public async Task<PagedResponse<CategoryResponse>> GetCategoriesPagedAsync(int page, int pageSize)
        {
            var categories = await _categoryRepository.GetPagedAsync(page, pageSize);
            var totalCount = await _categoryRepository.CountAsync();

            return PagedResponse<CategoryResponse>.Create(
                categories.ToCategoryResponseList(),
                page,
                pageSize,
                totalCount);
        }

       

        public async Task<int> GetCategoryCountAsync()
        {
            return await _categoryRepository.CountAsync();
        }

        #endregion

        #region Business Logic Operations

        public async Task<IEnumerable<CategorySummaryResponse>> GetCategoriesWithProductCountAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var categorySummaries = new List<CategorySummaryResponse>();

            foreach (var category in categories)
            {
                var productCount = await _productRepository.CountAsync(p => p.CategoryId == category.Id);
                categorySummaries.Add(category.ToCategorySummaryResponse(productCount));
            }

            return categorySummaries;
        }

        public async Task<IEnumerable<CategoryResponse>> GetTopCategoriesByProductCountAsync(int count)
        {
            var categoriesWithCounts = await GetCategoriesWithProductCountAsync();
            var topCategories = categoriesWithCounts
                .OrderByDescending(c => c.ProductCount)
                .Take(count)
                .Select(async c => await GetCategoryByIdAsync(c.Id))
                .Where(c => c.Result != null)
                .Select(c => c.Result!);

            return topCategories;
        }

        #endregion

    

        #region Validation

        public async Task<ValidationResult> ValidateCategoryAsync(CreateCategoryRequest request)
        {
            var errors = new List<ValidationError>();

            if (string.IsNullOrWhiteSpace(request.Name))
                errors.Add(new ValidationError(nameof(request.Name), "Category name is required"));

            // Check for duplicate name
            var existingCategory = await _categoryRepository.GetFirstOrDefaultAsync(c => c.Name == request.Name);
            if (existingCategory != null)
                errors.Add(new ValidationError(nameof(request.Name), "Category name already exists"));

            if (request.Name?.Length > 100)
                errors.Add(new ValidationError(nameof(request.Name), "Category name cannot exceed 100 characters"));

            if (request.Description?.Length > 500)
                errors.Add(new ValidationError(nameof(request.Description), "Category description cannot exceed 500 characters"));

            return new ValidationResult(errors.Count == 0, errors);
        }

        public async Task<ValidationResult> ValidateCategoryUpdateAsync(UpdateCategoryRequest request)
        {
            var errors = new List<ValidationError>();

            if (!await _categoryRepository.ExistsAsync(request.Id))
                errors.Add(new ValidationError(nameof(request.Id), "Category does not exist"));

            if (string.IsNullOrWhiteSpace(request.Name))
                errors.Add(new ValidationError(nameof(request.Name), "Category name is required"));

            // Check for duplicate name (excluding current category)
            var existingCategory = await _categoryRepository.GetFirstOrDefaultAsync(c => c.Name == request.Name && c.Id != request.Id);
            if (existingCategory != null)
                errors.Add(new ValidationError(nameof(request.Name), "Category name already exists"));

            if (request.Name?.Length > 100)
                errors.Add(new ValidationError(nameof(request.Name), "Category name cannot exceed 100 characters"));

            if (request.Description?.Length > 500)
                errors.Add(new ValidationError(nameof(request.Description), "Category description cannot exceed 500 characters"));

            return new ValidationResult(errors.Count == 0, errors);
        }

        #endregion

       

      

        
    }
}