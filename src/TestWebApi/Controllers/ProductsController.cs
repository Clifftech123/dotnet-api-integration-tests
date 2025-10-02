using Microsoft.AspNetCore.Mvc;
using TestWebApi.Shared.Services;
using TestWebApi.Shared.Constructs;

namespace TestWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _service;
        public ProductsController(IProductService service) => _service = service;

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ProductResponse>>> Create(CreateProductRequest request)
        {
            var created = await _service.CreateProductAsync(request);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, ApiResponse<ProductResponse>.SuccessResponse("Product created", created));
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductResponse>>>> GetAll()
        {
            var list = await _service.GetAllProductsAsync();
            return Ok(ApiResponse<IEnumerable<ProductResponse>>.SuccessResponse("Products retrieved", list));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<ProductResponse>>> Get(Guid id)
        {
            var product = await _service.GetProductByIdAsync(id);
            return Ok(ApiResponse<ProductResponse>.SuccessResponse("Product retrieved", product));
        }

        [HttpGet("{id:guid}/details")]
        public async Task<ActionResult<ApiResponse<ProductDetailsResponse>>> Details(Guid id)
        {
            var details = await _service.GetProductDetailsAsync(id);
            return Ok(ApiResponse<ProductDetailsResponse>.SuccessResponse("Product details retrieved", details!));
        }

        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductResponse>>>> Search([FromQuery] string term)
        {
            var results = await _service.SearchProductsAsync(term);
            return Ok(ApiResponse<IEnumerable<ProductResponse>>.SuccessResponse("Products search result", results));
        }

        [HttpGet("category/{categoryId:guid}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductResponse>>>> ByCategory(Guid categoryId)
        {
            var results = await _service.GetProductsByCategoryAsync(categoryId);
            return Ok(ApiResponse<IEnumerable<ProductResponse>>.SuccessResponse("Products by category", results));
        }

        [HttpGet("category/{categoryId:guid}/count")]
        public async Task<ActionResult<ApiResponse<object>>> CountByCategory(Guid categoryId)
        {
            var count = await _service.GetProductCountByCategoryAsync(categoryId);
            return Ok(ApiResponse<object>.SuccessResponse("Product count by category retrieved", new { categoryId, count }));
        }

        [HttpGet("price-range")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductResponse>>>> ByPrice([FromQuery] decimal minPrice, [FromQuery] decimal maxPrice)
        {
            var results = await _service.GetProductsByPriceRangeAsync(minPrice, maxPrice);
            return Ok(ApiResponse<IEnumerable<ProductResponse>>.SuccessResponse("Products by price range", results));
        }

        [HttpGet("paged")]
        public async Task<ActionResult<ApiResponse<PagedResponse<ProductResponse>>>> Paged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var paged = await _service.GetProductsPagedAsync(page, pageSize);
            return Ok(ApiResponse<PagedResponse<ProductResponse>>.SuccessResponse("Products page retrieved", paged));
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<ProductResponse>>> Update(Guid id, UpdateProductRequest request)
        {
            if (id != request.Id)
                return BadRequest(ApiResponse<ProductResponse>.ErrorResponse("Mismatched product id", default(ProductResponse)));

            var updated = await _service.UpdateProductAsync(request);
            return Ok(ApiResponse<ProductResponse>.SuccessResponse("Product updated", updated!));
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
        {
            await _service.DeleteProductAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse("Product deleted"));
        }
    }
}