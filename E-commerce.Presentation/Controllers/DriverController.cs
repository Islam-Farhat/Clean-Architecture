using E_commerce.Application.Features.Drivers.Commands;
using E_commerce.Application.Features.Drivers.Dtos;
using E_commerce.Application.Features.Drivers.Queries;
using E_commerce.Application.Features.Housemaid.Commands;
using E_commerce.Application.Features.Housemaids.Commands;
using E_commerce.Application.Features.Orders.Dtos;
using E_commerce.Application.Features.Orders.Queries;
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
    [Authorize(Roles = nameof(RoleSystem.Admin) + "," + nameof(RoleSystem.Driver) + "," + nameof(RoleSystem.Supervisor))]
    public class DriverController : ControllerBase
    {
        private readonly IMediator _mediator;
        public DriverController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create(AddDriverCommand addHousemaidCommand)
        {
            var result = await _mediator.Send(addHousemaidCommand);

            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.Error);
        }

        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> Update(UpdateDriverCommand updateHousemaid)
        {
            var result = await _mediator.Send(updateHousemaid);

            if (result.IsSuccess)
                return Ok();

            return BadRequest(result.Error);
        }

        [HttpDelete]
        [Route("delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteDriverCommand() { UserId = id });

            if (result.IsSuccess)
                return Ok();

            return BadRequest(result.Error);
        }

        [HttpGet]
        [Route("GetDrivers")]
        public async Task<List<GetDriversDto>> GetDrivers()
        {
            var drivers = await _mediator.Send(new GetAllDriversQuery { });

            return drivers;
        }

        [HttpPost]
        [Route("deliveredToHome")]
        public async Task<IActionResult> deliveredToHome(int id)
        {
            var result = await _mediator.Send(new DriverDeliveredHousemaidCommand { WorkingId = id });

            if (result.IsSuccess)
                return Ok();

            return BadRequest(result.Error);
        }

        [HttpPost]
        [Route("endOfShift")]
        public async Task<IActionResult> endOfShift(DriverEndOfShiftCommand driverEndOfShiftCommand)
        {
            var result = await _mediator.Send(driverEndOfShiftCommand);

            if (result.IsSuccess)
                return Ok();

            return BadRequest(result.Error);
        }

        [HttpGet]
        [Route("GetOrdersDriver")]
        [Authorize(Roles = nameof(RoleSystem.Driver))]
        public async Task<List<GetOrdersDto>> GetOrdersDriver(int skip = 0, int take = 10, string search = "", DateTime? workingDay = null, ShiftType? shiftType = null, OrderType? orderType = null)
        {
            var orders = await _mediator.Send(new GetOrdersDriverQuery
            {
                Skip = skip,
                Take = take,
                SearchParam = search,
                WorkingDay = workingDay,
                Shift = shiftType,
                OrderType = orderType
            });

            return orders;
        }
    }
}
