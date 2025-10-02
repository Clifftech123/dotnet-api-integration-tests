using TestWebApi.Core.Entities;
using TestWebApi.Shared.Constructs;
using TestWebApi.Shared.Extensions;
using TestWebApi.Shared.Repositories;
using TestWebApi.Shared.TestWebMiddleWare;
using ValidationError = TestWebApi.Shared.Constructs.ValidationError;

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

        public async Task<CategoryResponse?> GetCategoryByIdAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category is null)
                throw new CategoryNotFoundException(id);
            return category.ToCategoryResponse();
        }

        public async Task<CategoryDetailsResponse?> GetCategoryDetailsAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category is null)
                throw new CategoryNotFoundException(id);

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
            var validation = await ValidateCategoryAsync(request);
            if (!validation.IsValid)
            {
                // If duplicate name is detected in validation, raise specific exception
                var duplicate = validation.Errors.FirstOrDefault(e => e.Field == nameof(request.Name) && e.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase));
                if (duplicate.Field is not null)
                    throw new DuplicateCategoryNameException(request.Name!);

                throw new ValidationException(validation.Errors.Select(e => new TestWebMiddleWare.ValidationError { Field = e.Field, Message = e.Message }));
            }

            var entity = request.ToCategory();
            var created = await _categoryRepository.CreateAsync(entity);
            return created.ToCategoryResponse();
        }

        public async Task<CategoryResponse?> UpdateCategoryAsync(UpdateCategoryRequest request)
        {
            var validation = await ValidateCategoryUpdateAsync(request);
            if (!validation.IsValid)
            {
                var existsError = validation.Errors.FirstOrDefault(e => e.Field == nameof(request.Id) && e.Message.Contains("does not exist", StringComparison.OrdinalIgnoreCase));
                if (existsError?.Field is not null)
                    throw new CategoryNotFoundException(request.Id);

                var duplicate = validation.Errors.FirstOrDefault(e => e.Field == nameof(request.Name) && e.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase));
                if (duplicate?.Field is not null)
                    throw new DuplicateCategoryNameException(request.Name!);

                throw new ValidationException(validation.Errors.Select(e => new TestWebMiddleWare.ValidationError { Field = e.Field, Message = e.Message }));
            }

            var existing = await _categoryRepository.GetByIdAsync(request.Id);
            if (existing is null)
                throw new CategoryNotFoundException(request.Id);

            existing.UpdateFromRequest(request);
            await _categoryRepository.UpdateAsync(existing);
            return existing.ToCategoryResponse();
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category is null)
                throw new CategoryNotFoundException(id);

            var productCount = await _productRepository.CountAsync(p => p.CategoryId == id);
            if (productCount > 0)
                throw new CategoryDeleteNotAllowedException(id, productCount);

            return await _categoryRepository.DeleteAsync(id);
        }

        public async Task<bool> CategoryExistsAsync(Guid id) => await _categoryRepository.ExistsAsync(id);

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
            var result = new List<CategoryResponse>();
            foreach (var c in categories)
            {
                var hasProducts = await _productRepository.CountAsync(p => p.CategoryId == c.Id) > 0;
                if (hasProducts) result.Add(c.ToCategoryResponse());
            }
            return result;
        }

        public async Task<bool> CanDeleteCategoryAsync(Guid id)
        {
            var productCount = await _productRepository.CountAsync(p => p.CategoryId == id);
            return productCount == 0;
        }

        public async Task<PagedResponse<CategoryResponse>> GetCategoriesPagedAsync(int page, int pageSize)
        {
            var categories = await _categoryRepository.GetPagedAsync(page, pageSize);
            var total = await _categoryRepository.CountAsync();
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            if (page > totalPages && totalPages != 0)
                throw new PaginationOutOfRangeException(page, totalPages);

            return PagedResponse<CategoryResponse>.Create(categories.ToCategoryResponseList(), page, pageSize, total);
        }

        public async Task<int> GetCategoryCountAsync() => await _categoryRepository.CountAsync();

        public async Task<IEnumerable<CategorySummaryResponse>> GetCategoriesWithProductCountAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var list = new List<CategorySummaryResponse>();
            foreach (var c in categories)
            {
                var count = await _productRepository.CountAsync(p => p.CategoryId == c.Id);
                list.Add(c.ToCategorySummaryResponse(count));
            }
            return list;
        }

        public async Task<IEnumerable<CategoryResponse>> GetTopCategoriesByProductCountAsync(int count)
        {
            var summaries = await GetCategoriesWithProductCountAsync();
            var top = summaries
                .OrderByDescending(s => s.ProductCount)
                .Take(count)
                .Select(s => _categoryRepository.GetByIdAsync(s.Id).Result) // simple sync wait (could refactor)
                .Where(c => c != null)
                .Select(c => c!.ToCategoryResponse());
            return top;
        }

        public async Task<ValidationResult> ValidateCategoryAsync(CreateCategoryRequest request)
        {
            var errors = new List<ValidationError>();
            if (string.IsNullOrWhiteSpace(request.Name))
                errors.Add(new ValidationError(nameof(request.Name), "Category name is required"));
            var existing = await _categoryRepository.GetFirstOrDefaultAsync(c => c.Name == request.Name);
            if (existing != null)
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
            var existing = await _categoryRepository.GetFirstOrDefaultAsync(c => c.Name == request.Name && c.Id != request.Id);
            if (existing != null)
                errors.Add(new ValidationError(nameof(request.Name), "Category name already exists"));
            if (request.Name?.Length > 100)
                errors.Add(new ValidationError(nameof(request.Name), "Category name cannot exceed 100 characters"));
            if (request.Description?.Length > 500)
                errors.Add(new ValidationError(nameof(request.Description), "Category description cannot exceed 500 characters"));
            return new ValidationResult(errors.Count == 0, errors);
        }
    }
}