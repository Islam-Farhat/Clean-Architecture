using E_commerce.Application.Features.Orders;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Route("CreateNewOrder")]
        public async Task<IActionResult> CreateOrder(CreateOrderDto order)
        {
            var result = await _mediator.Send(new CreateOrderCommand
            {
                orderNumber = order.orderNumber,
                Items = order.Items,
            });

            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Value);
        }
    }
}
