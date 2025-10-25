using E_commerce.Application.Features.Drivers.Dtos;
using E_commerce.Application.Features.Drivers.Queries;
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

        [HttpGet]
        [Route("GetDrivers")]
        public async Task<List<GetDriversDto>> GetOrders()
        {
            var drivers = await _mediator.Send(new GetAllDriversQuery{});

            return drivers;
        }
    }
}
