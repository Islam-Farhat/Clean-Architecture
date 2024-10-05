using E_commerce.Application.Features.Users;
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
        [Route("RegisterUserAsync")]
        public async Task<IActionResult> RegisterUserAsync(RegisterUserCommand user)
        {
            var result = await _mediator.Send(user);

            if (result.IsSuccess)
                return Ok(result.Value);

            SetRefreshTokenInCookie(result.Value.RefreshToken, result.Value.RefreshTokenExpiration);

            return BadRequest(result.Error);
        }

        [HttpPost]
        [Route("LoginUserAsync")]
        public async Task<IActionResult> LoginUserAsync(LoginUserQuery user)
        {
            var result = await _mediator.Send(user);

            if (result.IsSuccess)
            {
                SetRefreshTokenInCookie(result.Value.RefreshToken, result.Value.RefreshTokenExpiration);
                return Ok(result.Value);
            }


            return BadRequest(result.Error);
        }

        [HttpGet]
        [Route("RefreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            var result = await _mediator.Send(new RefreshTokenQuery() { RefreshToken = refreshToken });

            if (!result.IsSuccess)
                return BadRequest(result);

            SetRefreshTokenInCookie(result.Value.RefreshToken, result.Value.RefreshTokenExpiration);

            return Ok(result.Value);
        }

        [HttpPost]
        [Route("RevokeToken")]

        public async Task<IActionResult> RevokeToken(string? revokeToken)
        {
            var token = revokeToken ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
                return BadRequest("Token is required!");

            var result = await _mediator.Send(new RevokeQuery() { RefreshToken = token });

            if (!result)
                return BadRequest("Token is invalid!");

            return Ok();
        }

        private void SetRefreshTokenInCookie(string refreshToken, DateTime expires)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expires.ToLocalTime(),
                SameSite = SameSiteMode.None
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}
