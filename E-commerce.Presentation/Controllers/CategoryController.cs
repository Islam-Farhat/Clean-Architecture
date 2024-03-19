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
        private readonly IMediator _mediator;

        public CategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

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

    }
}
