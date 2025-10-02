using Microsoft.AspNetCore.Mvc;
using TestWebApi.Shared.Services;
using TestWebApi.Shared.Constructs;

namespace TestWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _service;
        public CategoriesController(ICategoryService service) => _service = service;

        [HttpPost]
        public async Task<ActionResult<ApiResponse<CategoryResponse>>> Create(CreateCategoryRequest request)
        {
            var created = await _service.CreateCategoryAsync(request);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, ApiResponse<CategoryResponse>.SuccessResponse("Category created", created));
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<CategoryResponse>>>> GetAll()
        {
            var list = await _service.GetAllCategoriesAsync();
            return Ok(ApiResponse<IEnumerable<CategoryResponse>>.SuccessResponse("Categories retrieved", list));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<CategoryResponse>>> Get(Guid id)
        {
            var category = await _service.GetCategoryByIdAsync(id);
            return Ok(ApiResponse<CategoryResponse>.SuccessResponse("Category retrieved", category));
        }

        [HttpGet("{id:guid}/details")]
        public async Task<ActionResult<ApiResponse<CategoryDetailsResponse>>> Details(Guid id)
        {
            var details = await _service.GetCategoryDetailsAsync(id);
            return Ok(ApiResponse<CategoryDetailsResponse>.SuccessResponse("Category details retrieved", details!));
        }

        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CategoryResponse>>>> Search([FromQuery] string term)
        {
            var results = await _service.SearchCategoriesAsync(term);
            return Ok(ApiResponse<IEnumerable<CategoryResponse>>.SuccessResponse("Categories search result", results));
        }

        [HttpGet("paged")]
        public async Task<ActionResult<ApiResponse<PagedResponse<CategoryResponse>>>> Paged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var paged = await _service.GetCategoriesPagedAsync(page, pageSize);
            return Ok(ApiResponse<PagedResponse<CategoryResponse>>.SuccessResponse("Categories page retrieved", paged));
        }

        [HttpGet("with-products")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CategoryResponse>>>> WithProducts()
        {
            var data = await _service.GetCategoriesWithProductsAsync();
            return Ok(ApiResponse<IEnumerable<CategoryResponse>>.SuccessResponse("Categories with products retrieved", data));
        }

        [HttpGet("with-product-count")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CategorySummaryResponse>>>> WithProductCount()
        {
            var data = await _service.GetCategoriesWithProductCountAsync();
            return Ok(ApiResponse<IEnumerable<CategorySummaryResponse>>.SuccessResponse("Category counts retrieved", data));
        }

        [HttpGet("top")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CategoryResponse>>>> Top([FromQuery] int count = 5)
        {
            var data = await _service.GetTopCategoriesByProductCountAsync(count);
            return Ok(ApiResponse<IEnumerable<CategoryResponse>>.SuccessResponse("Top categories retrieved", data));
        }

        [HttpGet("{id:guid}/can-delete")]
        public async Task<ActionResult<ApiResponse<object>>> CanDelete(Guid id)
        {
            var can = await _service.CanDeleteCategoryAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse("Category delete status evaluated", new { canDelete = can }));
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<CategoryResponse>>> Update(Guid id, UpdateCategoryRequest request)
        {
            if (id != request.Id)
                return BadRequest(ApiResponse<CategoryResponse>.ErrorResponse("Mismatched category id", default(CategoryResponse)));

            var updated = await _service.UpdateCategoryAsync(request);
            return Ok(ApiResponse<CategoryResponse>.SuccessResponse("Category updated", updated!));
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
        {
            await _service.DeleteCategoryAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse("Category deleted"));
        }
    }
}