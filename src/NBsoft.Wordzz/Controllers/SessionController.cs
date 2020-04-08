﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NBsoft.Wordzz.Contracts.Requests;
using NBsoft.Wordzz.Contracts.Results;
using NBsoft.Wordzz.Core.Repositories;
using NBsoft.Wordzz.Core.Services;

namespace NBsoft.Wordzz.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SessionController : ControllerBase
    {
        private readonly ISessionService _sessionService;
        private readonly IUserRepository _userRepository;

        public SessionController(ISessionService sessionService, IUserRepository userRepository)
        {
            _sessionService = sessionService;
            _userRepository = userRepository;
        }

        [HttpPost]
        [Route("LogIn/")]
        [ProducesResponseType(typeof(LogInResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> LogIn([FromBody]LogInRequest request)
        {
            var session = await _sessionService.LogIn(request.UserName, request.Password, HttpContext.Connection.Id);
            if (session == null)
                return BadRequest(new { message = "Username or password is incorrect" });
                        
            var userDetails = await _userRepository.GetDetails(session.UserId);

            var result = new LogInResult {
                Username = session.UserId,
                FirstName = string.IsNullOrEmpty(userDetails.FirstName)? session.UserId: userDetails.FirstName,
                LastName = userDetails.LastName,
                Token = session.SessionToken
            };
            return Ok(result);            
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
