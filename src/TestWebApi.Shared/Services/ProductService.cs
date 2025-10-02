

using TestWebApi.Core.Entities;
using TestWebApi.Shared.Constructs;
using TestWebApi.Shared.Extensions;
using TestWebApi.Shared.Repositories;

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
        #region Basic CRUD Operations

        public async Task<ProductResponse?> GetProductByIdAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id, p => p.Category);
            return product?.ToProductResponse();
        }

        public async Task<ProductDetailsResponse?> GetProductDetailsAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id, p => p.Category);
            return product?.ToProductDetailsResponse();
        }

        public async Task<IEnumerable<ProductResponse>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllAsync(p => p.Category);
            return products.ToProductResponseList();
        }

        public async Task<ProductResponse> CreateProductAsync(CreateProductRequest request)
        {

            // Validate  product using the validation method
            var validationResult = await ValidateProductAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => $"{e.Field}: {e.Message}"));
                throw new ArgumentException($"Product validation failed: {errors}");
            }

            var product = request.ToProduct();
            var createdProduct = await _productRepository.CreateAsync(product);

            // Get the product with category for response
            var productWithCategory = await _productRepository.GetByIdAsync(createdProduct.Id, p => p.Category);
            return productWithCategory!.ToProductResponse();
        }

        public async Task<ProductResponse?> UpdateProductAsync(UpdateProductRequest request)
        {

            // Validate product using the validation method
            var validationResult = await ValidateProductUpdateAsync(request);
            if (!validationResult.IsValid)
                {
                var errors = string.Join("; ", validationResult.Errors.Select(e => $"{e.Field}: {e.Message}"));
                throw new ArgumentException($"Product validation failed: {errors}");
            }
            var existingProduct = await _productRepository.GetByIdAsync(request.Id);
            if (existingProduct == null)
                return null;

            existingProduct.UpdateFromRequest(request);
            await _productRepository.UpdateAsync(existingProduct);

            // Get updated product with category
            var updatedProduct = await _productRepository.GetByIdAsync(request.Id, p => p.Category);
            return updatedProduct!.ToProductResponse();
        }

        public async Task<bool> DeleteProductAsync(Guid id)
        {
            return await _productRepository.DeleteAsync(id);
        }

        #endregion



        #region Advanced Operations

        public async Task<bool> ProductExistsAsync(Guid id)
        {
            return await _productRepository.ExistsAsync(id);
        }

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

        #endregion


        #region Pagination & Filtering

        public async Task<PagedResponse<ProductResponse>> GetProductsPagedAsync(int page, int pageSize)
        {
            var products = await _productRepository.GetPagedAsync(page, pageSize, p => p.Category);
            var totalCount = await _productRepository.CountAsync();

            return PagedResponse<ProductResponse>.Create(
                products.ToProductResponseList(),
                page,
                pageSize,
                totalCount);
        }

       
        public async Task<int> GetProductCountAsync()
        {
            return await _productRepository.CountAsync();
        }

        public async Task<int> GetProductCountByCategoryAsync(Guid categoryId)
        {
            return await _productRepository.CountAsync(p => p.CategoryId == categoryId);
        }

        #endregion

    
    
        #region Validation

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

        #endregion

        #region Private Helper Methods

   

      

    

        #endregion
    }
}
