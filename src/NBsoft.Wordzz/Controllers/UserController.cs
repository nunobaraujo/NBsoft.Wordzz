using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Contracts.Requests;
using NBsoft.Wordzz.Contracts.Settings;
using NBsoft.Wordzz.Core.Repositories;
using NBsoft.Wordzz.Core.Services;
using NBsoft.Wordzz.Entities;
using NBsoft.Wordzz.Extensions;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ISessionService _sessionService;

        public UserController(IUserRepository userRepository, ISessionService sessionService)
        {            
            _userRepository = userRepository;
            _sessionService = sessionService;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("Create/")]
        [ProducesResponseType(typeof(User), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Create([FromBody]LogInRequest request)
        {
            var user = new User {
                CreationDate = DateTime.UtcNow,
                Deleted = false,
                UserName = request.UserName,
                PasswordHash = "",
                Salt = ""
            };

            var newUser = await _userRepository.Add(user, request.Email);
            await _userRepository.SetPassword(request.UserName, request.Password);
            
           
            return Ok(newUser);
        }

        [HttpGet]
        [Route("MainSettings/")]
        [ProducesResponseType(typeof(MainSettings), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> MainSettings()
        {
            var result = await HttpContext.AuthenticateAsync();
            string accessToken = result.Properties.Items[".Token.access_token"];
            var session = await _sessionService.GetSession(accessToken);
            if (session == null)
                return BadRequest(new { message = "Session expired. Please login again." });

            var settings = await _userRepository.GetSettings(session.UserId);
            var mainSettings = settings?.MainSettings?.FromJson<MainSettings>();

            return Ok(mainSettings);
        }

    }
}
