using E_commerce.Application.Features.Orders.Dtos;
using E_commerce.Application.Features.Orders.Queries;
using E_commerce.Application.Features.Reports.Dtos;
using E_commerce.Application.Features.Reports.Queries;
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
    [Authorize(Roles = nameof(RoleSystem.Admin) + "," + nameof(RoleSystem.Supervisor))]
    public class ReportsController
    {
        private readonly IMediator _mediator;
        public ReportsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("DriverOrdersInsights")]
        public async Task<DriverOrdersDto> DriverOrdersInsights(int driverId, DateTime fromDate, DateTime toDate, int skip = 0, int take = 10, ShiftType? shiftType = null, OrderType? orderType = null)
        {
            var orders = await _mediator.Send(new GetDriverOrdersQuery
            {
                Skip = skip,
                Take = take,
                OrderType = orderType,
                ShiftType = shiftType,
                DriverId = driverId,
                FromDate = fromDate,
                ToDate = toDate
            });

            return orders;
        }

        [HttpGet]
        [Route("DataEntryOrdersInsights")]
        public async Task<DataEntryOrdersDto> DataEntryOrdersInsights(int dataEntryId, DateTime fromDate, DateTime toDate, int skip = 0, int take = 10, ShiftType? shiftType = null, OrderType? orderType = null)
        {
            var orders = await _mediator.Send(new GetDataEntryOrdersQuery
            {
                Skip = skip,
                Take = take,
                OrderType = orderType,
                ShiftType = shiftType,
                DataEntryId = dataEntryId,
                FromDate = fromDate,
                ToDate = toDate
            });

            return orders;
        }
    }
}
