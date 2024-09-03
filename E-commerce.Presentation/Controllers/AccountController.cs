using E_commerce.Application.Features.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Presentation.Controllers
{
    [Authorize(Roles = "User")]
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
        [Route("RegisterUserAsync")]
        public async Task<IActionResult> RegisterUserAsync(RegisterUserCommand user)
        {
            var result = await _mediator.Send(user);

            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.Error);
        }

        [HttpPost]
        [Route("LoginUserAsync")]
        public async Task<IActionResult> LoginUserAsync(LoginUserQuery user)
        {
            var result = await _mediator.Send(user);

            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.Error);
        }

    }
}
