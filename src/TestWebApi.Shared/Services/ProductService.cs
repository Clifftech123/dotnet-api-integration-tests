using TestWebApi.Core.Entities;
using TestWebApi.Shared.Constructs;
using TestWebApi.Shared.Extensions;
using TestWebApi.Shared.Middleware;
using TestWebApi.Shared.Repositories;
using TestWebApi.Shared.TestWebMiddleWare;
using ValidationError = TestWebApi.Shared.Constructs.ValidationError;

namespace TestWebApi.Shared.Services
{
    public class ProductService : IProductService
    {
        private readonly IGenericRepository<Product> _productRepository;
        private readonly IGenericRepository<Category> _categoryRepository;

        public ProductService(IGenericRepository<Product> productRepository, IGenericRepository<Category> categoryRepository)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        }

        public async Task<ProductResponse?> GetProductByIdAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id, p => p.Category);
            if (product is null)
                throw new ProductNotFoundException(id);
            return product.ToProductResponse();
        }

        public async Task<ProductDetailsResponse?> GetProductDetailsAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id, p => p.Category);
            if (product is null)
                throw new ProductNotFoundException(id);
            return product.ToProductDetailsResponse();
        }

        public async Task<IEnumerable<ProductResponse>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllAsync(p => p.Category);
            return products.ToProductResponseList();
        }

        public async Task<ProductResponse> CreateProductAsync(CreateProductRequest request)
        {
            var validation = await ValidateProductAsync(request);
            if (!validation.IsValid)
                throw new ValidationException(validation.Errors.Select(e => new TestWebMiddleWare.ValidationError { Field = e.Field, Message = e.Message }));

            var entity = request.ToProduct();
            var created = await _productRepository.CreateAsync(entity);

            var withCategory = await _productRepository.GetByIdAsync(created.Id, p => p.Category);
            return withCategory!.ToProductResponse();
        }

        public async Task<ProductResponse?> UpdateProductAsync(UpdateProductRequest request)
        {
            var validation = await ValidateProductUpdateAsync(request);
            if (!validation.IsValid)
                throw new ValidationException(validation.Errors.Select(e => new TestWebMiddleWare.ValidationError { Field = e.Field, Message = e.Message }));

            var existing = await _productRepository.GetByIdAsync(request.Id);
            if (existing is null)
                throw new ProductNotFoundException(request.Id);

            existing.UpdateFromRequest(request);
            await _productRepository.UpdateAsync(existing);

            var updated = await _productRepository.GetByIdAsync(request.Id, p => p.Category);
            return updated!.ToProductResponse();
        }

        public async Task<bool> DeleteProductAsync(Guid id)
        {
            var exists = await _productRepository.ExistsAsync(id);
            if (!exists)
                throw new ProductNotFoundException(id);
            return await _productRepository.DeleteAsync(id);
        }

        public async Task<bool> ProductExistsAsync(Guid id) => await _productRepository.ExistsAsync(id);

        public async Task<IEnumerable<ProductResponse>> GetProductsByCategoryAsync(Guid categoryId)
        {
            var products = await _productRepository.FindAsync(p => p.CategoryId == categoryId, p => p.Category);
            return products.ToProductResponseList();
        }

        public async Task<IEnumerable<ProductResponse>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            var products = await _productRepository.FindAsync(
                p => p.Price >= minPrice && p.Price <= maxPrice,
                p => p.Category);
            return products.ToProductResponseList();
        }

        public async Task<IEnumerable<ProductResponse>> SearchProductsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllProductsAsync();

            var products = await _productRepository.FindAsync(
                p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm),
                p => p.Category);
            return products.ToProductResponseList();
        }

        public async Task<PagedResponse<ProductResponse>> GetProductsPagedAsync(int page, int pageSize)
        {
            var products = await _productRepository.GetPagedAsync(page, pageSize, p => p.Category);
            var total = await _productRepository.CountAsync();
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            if (page > totalPages && totalPages != 0)
                throw new PaginationOutOfRangeException(page, totalPages);

            return PagedResponse<ProductResponse>.Create(products.ToProductResponseList(), page, pageSize, total);
        }

        public async Task<int> GetProductCountAsync() => await _productRepository.CountAsync();
        public async Task<int> GetProductCountByCategoryAsync(Guid categoryId) =>
            await _productRepository.CountAsync(p => p.CategoryId == categoryId);

        // Validation methods unchanged (still return ValidationResult for reuse)
        public async Task<ValidationResult> ValidateProductAsync(CreateProductRequest request)
        {
            var errors = new List<ValidationError>();
            if (string.IsNullOrWhiteSpace(request.Name))
                errors.Add(new ValidationError(nameof(request.Name), "Product name is required"));
            if (request.Price <= 0)
                errors.Add(new ValidationError(nameof(request.Price), "Product price must be greater than 0"));
            if (!await _categoryRepository.ExistsAsync(request.CategoryId))
                errors.Add(new ValidationError(nameof(request.CategoryId), "Category does not exist"));
            return new ValidationResult(errors.Count == 0, errors);
        }

        public async Task<ValidationResult> ValidateProductUpdateAsync(UpdateProductRequest request)
        {
            var errors = new List<ValidationError>();
            if (!await _productRepository.ExistsAsync(request.Id))
                errors.Add(new ValidationError(nameof(request.Id), "Product does not exist"));
            if (string.IsNullOrWhiteSpace(request.Name))
                errors.Add(new ValidationError(nameof(request.Name), "Product name is required"));
            if (request.Price <= 0)
                errors.Add(new ValidationError(nameof(request.Price), "Product price must be greater than 0"));
            if (!await _categoryRepository.ExistsAsync(request.CategoryId))
                errors.Add(new ValidationError(nameof(request.CategoryId), "Category does not exist"));
            return new ValidationResult(errors.Count == 0, errors);
        }
    }
}