using E_commerce.Application;
using E_commerce.Application.Dto.Category;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IMediator _mediator;

        public CategoryController(ICategoryService categoryService, IMediator mediator)
        {
            this._categoryService = categoryService;
            _mediator = mediator;
        }

        //Using Mediator
        [HttpPost("AddWithMediator")]
        public async Task<IActionResult> Add(CreateCategoryCommand category)
        {
            var result = await _mediator.Send(category);
            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);

        }

        [HttpGet("GetAllWithMediator")]
        public async Task<IActionResult> GetAllWithMediator()
        {
            var query = new GetAllCategoriesQuery();
            var result = await _mediator.Send(query);
            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);

        }


        //Using Services
        [HttpPost("Add")]
        public async Task<IActionResult> Add(CreateCategoryDto category)
        {
            var result = await _categoryService.Add(category);
            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);

        }

        [HttpGet("GetObj")]
        public async Task<IActionResult> GetObj([FromQuery] GetCategoryDto categoryDto)
        {
            var result = await _categoryService.GetObj(categoryDto);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);

        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _categoryService.GetAll();
            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);

        }
    }
}
