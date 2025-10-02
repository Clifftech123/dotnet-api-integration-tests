using TestWebApi.Core.Entities;
using TestWebApi.Shared.Constructs;

namespace TestWebApi.Shared.Extensions
{
    public static class ProductMappingExtension
    {
        // ================================
        // PRODUCT ENTITY TO DTOs
        // ================================

        public static ProductResponse ToProductResponse(this Product product)
        {
            return new ProductResponse(
                Id: product.Id,
                Name: product.Name,
                Price: product.Price,
                Description: product.Description,
                CategoryId: product.CategoryId,
                CategoryName: product.Category?.Name ?? string.Empty,
                CreatedAt: product.CreatedAt,
                UpdatedAt: product.UpdatedAt
            );
        }

        public static ProductDetailsResponse ToProductDetailsResponse(this Product product)
        {
            return new ProductDetailsResponse(
                Id: product.Id,
                Name: product.Name,
                Price: product.Price,
                Description: product.Description,
                CategoryId: product.CategoryId,
                CategoryName: product.Category?.Name ?? string.Empty,
                CreatedAt: product.CreatedAt,
                UpdatedAt: product.UpdatedAt
            );
        }

        public static ProductDto ToProductDto(this Product product)
        {
            return new ProductDto(
                Id: product.Id,
                Name: product.Name,
                Price: product.Price,
                Description: product.Description,
                CategoryId: product.CategoryId,
                CategoryName: product.Category?.Name ?? string.Empty,
                CreatedAt: product.CreatedAt,
                UpdatedAt: product.UpdatedAt
            );
        }

        public static ProductSummaryResponse ToProductSummaryResponse(this Product product)
        {
            return new ProductSummaryResponse(
                Id: product.Id,
                Name: product.Name,
                Price: product.Price,
                CategoryName: product.Category?.Name ?? string.Empty
            );
        }

        // ================================
        // REQUEST DTOs TO PRODUCT ENTITY
        // ================================

        public static Product ToProduct(this CreateProductRequest request)
        {
            return new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Price = request.Price,
                Description = request.Description ?? string.Empty,
                CategoryId = request.CategoryId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static Product ToProduct(this UpdateProductRequest request)
        {
            return new Product
            {
                Id = request.Id,
                Name = request.Name,
                Price = request.Price,
                Description = request.Description ?? string.Empty,
                CategoryId = request.CategoryId,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static void UpdateFromRequest(this Product product, UpdateProductRequest request)
        {
            product.Name = request.Name;
            product.Price = request.Price;
            product.Description = request.Description ?? string.Empty;
            product.CategoryId = request.CategoryId;
            product.UpdatedAt = DateTime.UtcNow;
        }

        // ================================
        // COLLECTION MAPPINGS
        // ================================

        public static IEnumerable<ProductResponse> ToProductResponseList(this IEnumerable<Product> products)
        {
            return products.Select(product => product.ToProductResponse());
        }

        public static List<ProductResponse> ToProductResponseList(this List<Product> products)
        {
            return products.Select(product => product.ToProductResponse()).ToList();
        }

        public static IEnumerable<ProductDto> ToProductDtoList(this IEnumerable<Product> products)
        {
            return products.Select(product => product.ToProductDto());
        }

        public static List<ProductDto> ToProductDtoList(this List<Product> products)
        {
            return products.Select(product => product.ToProductDto()).ToList();
        }

        public static IEnumerable<ProductSummaryResponse> ToProductSummaryResponseList(this IEnumerable<Product> products)
        {
            return products.Select(product => product.ToProductSummaryResponse());
        }


     

        // ================================
        // PAGINATION MAPPINGS
        // ================================

        public static ProductListResponse ToProductListResponse(this IEnumerable<Product> products, int totalCount, int page, int pageSize)
        {
            return new ProductListResponse(
                Products: products.ToProductResponseList(),
                TotalCount: totalCount,
                Page: page,
                PageSize: pageSize
            );
        }

        public static PagedResponse<ProductResponse> ToPagedProductResponse(this IEnumerable<Product> products, int page, int pageSize, int totalCount)
        {
            return PagedResponse<ProductResponse>.Create(
                products.ToProductResponseList(),
                page,
                pageSize,
                totalCount
            );
        }
    }

    public static class CategoryMappingExtension
    {
        // ================================
        // CATEGORY ENTITY TO DTOs
        // ================================

        public static CategoryResponse ToCategoryResponse(this Category category)
        {
            return new CategoryResponse(
                Id: category.Id,
                Name: category.Name,
                Description: category.Description,
                CreatedAt: category.CreatedAt,
                UpdatedAt: category.UpdatedAt
            );
        }

        public static CategoryDetailsResponse ToCategoryDetailsResponse(this Category category, IEnumerable<Product>? products = null)
        {
            return new CategoryDetailsResponse(
                Id: category.Id,
                Name: category.Name,
                Description: category.Description,
                CreatedAt: category.CreatedAt,
                UpdatedAt: category.UpdatedAt,
                Products: products?.ToProductSummaryResponseList()
            );
        }

        public static CategoryDto ToCategoryDto(this Category category)
        {
            return new CategoryDto(
                Id: category.Id,
                Name: category.Name,
                Description: category.Description,
                CreatedAt: category.CreatedAt,
                UpdatedAt: category.UpdatedAt
            );
        }

        public static CategorySummaryResponse ToCategorySummaryResponse(this Category category, int productCount = 0)
        {
            return new CategorySummaryResponse(
                Id: category.Id,
                Name: category.Name,
                ProductCount: productCount
            );
        }

        // ================================
        // REQUEST DTOs TO CATEGORY ENTITY
        // ================================

        public static Category ToCategory(this CreateCategoryRequest request)
        {
            return new Category
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static Category ToCategory(this UpdateCategoryRequest request)
        {
            return new Category
            {
                Id = request.Id,
                Name = request.Name,
                Description = request.Description ?? string.Empty,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static void UpdateFromRequest(this Category category, UpdateCategoryRequest request)
        {
            category.Name = request.Name;
            category.Description = request.Description ?? string.Empty;
            category.UpdatedAt = DateTime.UtcNow;
        }

        // ================================
        // COLLECTION MAPPINGS
        // ================================

        public static IEnumerable<CategoryResponse> ToCategoryResponseList(this IEnumerable<Category> categories)
        {
            return categories.Select(category => category.ToCategoryResponse());
        }

        public static List<CategoryResponse> ToCategoryResponseList(this List<Category> categories)
        {
            return categories.Select(category => category.ToCategoryResponse()).ToList();
        }

        public static IEnumerable<CategoryDto> ToCategoryDtoList(this IEnumerable<Category> categories)
        {
            return categories.Select(category => category.ToCategoryDto());
        }

        public static List<CategoryDto> ToCategoryDtoList(this List<Category> categories)
        {
            return categories.Select(category => category.ToCategoryDto()).ToList();
        }

        public static IEnumerable<CategorySummaryResponse> ToCategorySummaryResponseList(this IEnumerable<Category> categories)
        {
            return categories.Select(category => category.ToCategorySummaryResponse());
        }

       

        // ================================
        // PAGINATION MAPPINGS
        // ================================

        public static CategoryListResponse ToCategoryListResponse(this IEnumerable<Category> categories, int totalCount, int page, int pageSize)
        {
            return new CategoryListResponse(
                Categories: categories.ToCategoryResponseList(),
                TotalCount: totalCount,
                Page: page,
                PageSize: pageSize
            );
        }

        public static PagedResponse<CategoryResponse> ToPagedCategoryResponse(this IEnumerable<Category> categories, int page, int pageSize, int totalCount)
        {
            return PagedResponse<CategoryResponse>.Create(
                categories.ToCategoryResponseList(),
                page,
                pageSize,
                totalCount
            );
        }
    }

   
    
}