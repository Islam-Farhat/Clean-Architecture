using E_commerce.Application;
using E_commerce.Application.Dto.Category;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            this._categoryService = categoryService;
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Add(CreateCategoryDto category)
        {
            var result = await _categoryService.Add(category);
            if (result.IsSuccess)
                return Ok(result.ErrorMessege);

            return BadRequest(result.ErrorMessege);

        }

        [HttpGet("GetObj")]
        public async Task<IActionResult> GetObj([FromQuery] GetCategoryDto categoryDto)
        {
            var result = await _categoryService.GetObj(categoryDto);

            if (result.IsSuccess)
                return Ok(result.Data);

            return BadRequest(result.ErrorMessege);

        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _categoryService.GetAll();
            if (result.IsSuccess)
                return Ok(result.Data);

            return BadRequest(result.ErrorMessege);

        }
    }
}
