using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NBsoft.Wordzz.Contracts.Requests;
using NBsoft.Wordzz.Core.Services;

namespace NBsoft.Wordzz.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SessionController : ControllerBase
    {
        private readonly ISessionService _sessionService;

        public SessionController(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        [HttpPost]
        [Route("LogIn/")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> LogIn([FromBody]LogInRequest request)
        {
            var user = await _sessionService.LogIn(request.UserName, request.Password, HttpContext.Connection.Id);
            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(user);
            
        }
        [Authorize]
        [HttpDelete]
        [Route("LogIn/")]
        public async Task<IActionResult> LogOut()
        {
            var result = await HttpContext.AuthenticateAsync();
            string accessToken = result.Properties.Items[".Token.access_token"];
            await _sessionService.LogOut(accessToken);
            return Ok();
        }
    }
}
