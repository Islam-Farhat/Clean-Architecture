using E_commerce.Application.Features.Orders.Commands;
using E_commerce.Application.Features.Orders.Dtos;
using E_commerce.Application.Features.Orders.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        [Route("AddNewOrder")]
        public async Task<IActionResult> AddNewOrder(AddNewOrderCommand addNewOrderCommand)
        {
            var result = await _mediator.Send(addNewOrderCommand);

            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.Error);
        }

        [HttpGet]
        [Route("GetOrders")]
        public async Task<List<GetOrdersDto>> GetOrders(int skip = 0, int take = 10, string search = "", DateTime? workingDay = null)
        {
            var saudiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
            var workingDayValue = workingDay ?? TimeZoneInfo.ConvertTime(DateTime.UtcNow, saudiTimeZone).Date;

            var orders = await _mediator.Send(new GetOrderQuery
            {
                Skip = skip,
                SearchParam = search,
                Take = take,
                WorkingDay = workingDayValue
            });

            return orders;
        }

        [HttpDelete]
        [Route("delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteOrderCommand() { Id = id });

            if (result.IsSuccess)
                return Ok();

            return BadRequest(result.Error);
        }


        [HttpPost]
        [Route("AssignOrderToDriver")]
        public async Task<IActionResult> AssignOrderToDriver(int driverId, int workingId)
        {
            var result = await _mediator.Send(new AssignOrderToDriverCommand
            {
                DriverId = driverId,
                WorkingOrderId = workingId
            });

            if (result.IsSuccess)
                return Ok();

            return BadRequest(result.Error);
        }

    }
}
