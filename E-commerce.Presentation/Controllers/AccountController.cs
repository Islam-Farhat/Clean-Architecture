using E_commerce.Application.Features.Users.Commands;
using E_commerce.Application.Features.Users.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AccountController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Route("register-user")]
        public async Task<IActionResult> RegisterUserAsync(RegisterUserCommand user)
        {
            var result = await _mediator.Send(user);

            if (result.IsSuccess)
                return Ok();

            return BadRequest(result.Error);
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> LoginUser([FromBody] LoginUserCommand user)
        {
            var result = await _mediator.Send(user);

            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.Error);
        }

        [HttpPost]
        [Route("refresh-token")]
        public async Task<ActionResult<AuthenticationResponseDto>> Refresh([FromBody] RefreshTokenCommand command)
        {
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.Error);
        }
    }
}
