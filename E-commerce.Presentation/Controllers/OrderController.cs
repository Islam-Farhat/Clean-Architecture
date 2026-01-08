using E_commerce.Application.Common;
using E_commerce.Application.Features.Orders.Commands;
using E_commerce.Application.Features.Orders.Dtos;
using E_commerce.Application.Features.Orders.Queries;
using E_commerce.Application.Interfaces;
using E_commerce.Domian.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ISessionUserService _sessionUser;
        public OrderController(IMediator mediator, ISessionUserService sessionUser)
        {
            _mediator = mediator;
            _sessionUser = sessionUser;
        }


        [HttpPost]
        [Route("AddNewOrder")]
        [Authorize(Roles = nameof(RoleSystem.Admin) + "," + nameof(RoleSystem.DataEntry))]
        public async Task<IActionResult> AddNewOrder(AddNewOrderCommand addNewOrderCommand)
        {
            var result = await _mediator.Send(addNewOrderCommand);

            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.Error);
        }

        [HttpPut]
        [Route("EditOrder")]
        [Authorize(Roles = nameof(RoleSystem.Admin) + "," + nameof(RoleSystem.DataEntry))]
        public async Task<IActionResult> EditOrder(EditOrderCommand editOrderCommand)
        {
            var result = await _mediator.Send(editOrderCommand);

            if (result.IsSuccess)
                return Ok();

            return BadRequest(result.Error);
        }

        [HttpGet]
        [Route("GetOrders")]
        [Authorize(Roles = nameof(RoleSystem.Admin) + "," + nameof(RoleSystem.DataEntry) + "," + nameof(RoleSystem.Supervisor))]
        public async Task<List<GetOrdersDto>> GetOrders(int skip = 0, int take = 10, string search = "", DateTime? workingDay = null, ShiftType? shiftType = null, OrderType? orderType = null, bool? isAssigned = null, bool? includeStayIn = null)
        {
            var orders = await _mediator.Send(new GetOrderQuery
            {
                Skip = skip,
                Take = take,
                SearchParam = search,
                WorkingDay = workingDay,
                Shift = shiftType,
                OrderType = orderType,
                IsAssigned = isAssigned.HasValue ? isAssigned.Value : false,
                IncludeStayIn = includeStayIn.HasValue ? includeStayIn.Value : false,
            });

            return orders;
        }

        [HttpDelete]
        [Route("delete")]
        [Authorize(Roles = nameof(RoleSystem.Admin))]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteOrderCommand() { Id = id });

            if (result.IsSuccess)
                return Ok();

            return BadRequest(result.Error);
        }

        [HttpPost]
        [Route("cancelOrder")]
        [Authorize(Roles = nameof(RoleSystem.Admin) + "," + nameof(RoleSystem.DataEntry))]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var result = await _mediator.Send(new CancelOrderCommand() { Id = id });

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
