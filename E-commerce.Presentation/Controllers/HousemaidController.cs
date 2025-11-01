using E_commerce.Application.Features.Housemaid.Commands;
using E_commerce.Application.Features.Housemaid.Queries;
using E_commerce.Application.Features.Housemaids.Commands;
using E_commerce.Domian.Enums;
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
    public class HousemaidController : ControllerBase
    {
        private readonly IMediator _mediator;
        public HousemaidController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create(AddHousemaidCommand addHousemaidCommand)
        {
            var result = await _mediator.Send(addHousemaidCommand);

            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.Error);
        }

        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> Create(UpdateHousemaidCommand updateHousemaid)
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
            var result = await _mediator.Send(new DeleteHousemaidCommand() { Id = id });

            if (result.IsSuccess)
                return Ok();

            return BadRequest(result.Error);
        }

        [HttpGet]
        [Route("getHousemaidById")]
        public async Task<IActionResult> GetHousemaidById(int id)
        {
            var result = await _mediator.Send(new GetHousemaidByIdCommand() { Id = id });

            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.Error);
        }

        [HttpGet]
        [Route("getHousemaids")]
        public async Task<IActionResult> getHousemaids(int skip = 0, int take = 10, string searchParam = "")
        {
            var result = await _mediator.Send(new GetHousemaidsCommand()
            {
                Take = take,
                SearchParam = searchParam,
                Skip = skip,
            });

            return Ok(result);
        }

        [HttpGet]
        [Route("getAvailableHousemaids")]
        public async Task<IActionResult> getHousemaids(ShiftType Shift, OrderType orderType, [FromQuery] List<DateTime> workingDays)
        {
            var result = await _mediator.Send(new GetAvailableHousemaidsQuery()
            {
                Shift = Shift,
                WorkingDays = workingDays,
                OrderType = orderType
            });

            return Ok(result);
        }
    }
}
