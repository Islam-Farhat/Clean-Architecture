using E_commerce.Application.Features.Drivers.Commands;
using E_commerce.Application.Features.Drivers.Dtos;
using E_commerce.Application.Features.Drivers.Queries;
using E_commerce.Application.Features.Housemaid.Commands;
using E_commerce.Application.Features.Housemaids.Commands;
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
            var drivers = await _mediator.Send(new GetAllDriversQuery{});

            return drivers;
        }
    }
}
