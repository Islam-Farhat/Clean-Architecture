using E_commerce.Application.Features.DataEntry.Dtos;
using E_commerce.Application.Features.DataEntry.Queries;
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
    [Authorize]
    public class DataEntryController : ControllerBase
    {
        private readonly IMediator _mediator;
        public DataEntryController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpGet]
        [Route("GetAllDataEntries")]
        public async Task<List<GetDataEntryDto>> GetAllDataEntries()
        {
            var drivers = await _mediator.Send(new GetAllDataEntriesQuery { });

            return drivers;
        }

        [HttpGet]
        [Route("GetOrdersDataEntry")]
        public async Task<List<GetOrdersDto>> GetOrdersDataEntry(int skip = 0, int take = 10, string search = "", DateTime? workingDay = null, ShiftType? shiftType = null, OrderType? orderType = null, int userId =0)
        {
            var orders = await _mediator.Send(new GetOrdersDataEntryQuery
            {
                Skip = skip,
                Take = take,
                SearchParam = search,
                WorkingDay = workingDay,
                Shift = shiftType,
                OrderType = orderType,
                UserId = userId
            });

            return orders;
        }
    }
}
