namespace TestWebApi.Shared.Constructs
{
    // ================================
    // PRODUCT DTOs
    // ================================

    // Product Request DTOs
    public record CreateProductRequest(string Name, decimal Price, string? Description, Guid CategoryId);
    public record UpdateProductRequest(Guid Id, string Name, decimal Price, string? Description, Guid CategoryId);
    public record DeleteProductRequest(Guid Id);
    public record GetProductRequest(Guid Id);
    public record GetProductsByCategoryRequest(Guid CategoryId);

    // Product Response DTOs
    public record ProductResponse(Guid Id, string Name, decimal Price, string? Description, Guid CategoryId, string CategoryName, DateTime CreatedAt, DateTime UpdatedAt);
    public record ProductDetailsResponse(Guid Id, string Name, decimal Price, string? Description, Guid CategoryId, string CategoryName, DateTime CreatedAt, DateTime UpdatedAt);
    public record ProductDto(Guid Id, string Name, decimal Price, string? Description, Guid CategoryId, string CategoryName, DateTime CreatedAt, DateTime UpdatedAt);

    // Product List Response DTOs
    public record ProductListResponse(IEnumerable<ProductResponse> Products, int TotalCount, int Page, int PageSize);
    public record ProductSummaryResponse(Guid Id, string Name, decimal Price, string CategoryName);

    // ================================
    // CATEGORY DTOs
    // ================================

    // Category Request DTOs
    public record CreateCategoryRequest(string Name, string? Description);
    public record UpdateCategoryRequest(Guid Id, string Name, string? Description);
    public record DeleteCategoryRequest(Guid Id);
    public record GetCategoryRequest(Guid Id);

    // Category Response DTOs
    public record CategoryResponse(Guid Id, string Name, string? Description, DateTime CreatedAt, DateTime UpdatedAt);
    public record CategoryDetailsResponse(Guid Id, string Name, string? Description, DateTime CreatedAt, DateTime UpdatedAt, IEnumerable<ProductSummaryResponse>? Products = null);
    public record CategoryDto(Guid Id, string Name, string? Description, DateTime CreatedAt, DateTime UpdatedAt);

    // Category List Response DTOs
    public record CategoryListResponse(IEnumerable<CategoryResponse> Categories, int TotalCount, int Page, int PageSize);
    public record CategorySummaryResponse(Guid Id, string Name, int ProductCount);

   

    // Pagination DTOs
    public record PaginationRequest(int Page = 1, int PageSize = 10, string? Search = null, string? SortBy = null, bool SortDescending = false);

    public record PagedResponse<T>(
        IEnumerable<T> Items,
        int Page,
        int PageSize,
        int TotalCount,
        int TotalPages,
        bool HasPreviousPage,
        bool HasNextPage
    )
    {
        public static PagedResponse<T> Create(IEnumerable<T> items, int page, int pageSize, int totalCount)
        {
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            return new PagedResponse<T>(
                items,
                page,
                pageSize,
                totalCount,
                totalPages,
                page > 1,
                page < totalPages
            );
        }
    }

    // ================================
    // VALIDATION DTOs
    // ================================

    public record ValidationError(string Field, string Message);
    public record ValidationResult(bool IsValid, IEnumerable<ValidationError> Errors);


  

   
}