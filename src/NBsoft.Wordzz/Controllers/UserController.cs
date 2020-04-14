using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NBsoft.Logs;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Contracts.Requests;
using NBsoft.Wordzz.Contracts.Settings;
using NBsoft.Wordzz.Core.Repositories;
using NBsoft.Wordzz.Core.Services;
using NBsoft.Wordzz.Entities;
using NBsoft.Wordzz.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly ILogger _log;

        public UserController(IUserRepository userRepository, ISessionService sessionService, ILogger log)
        {            
            _userRepository = userRepository;
            _sessionService = sessionService;
            _log = log;
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

            await _log.InfoAsync($"New User Created: [{newUser.UserName}]");
            return Ok(newUser);
        }

        [HttpGet]
        [Route("MainSettings/")]
        [ProducesResponseType(typeof(MainSettings), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> MainSettings()
        {
            string accessToken = await HttpContext.GetToken();
            var session = await _sessionService.GetSession(accessToken);
            if (session == null)
                return BadRequest(new { message = "Session expired. Please login again." });

            var settings = await _userRepository.GetSettings(session.UserId);
            var mainSettings = settings?.MainSettings?.FromJson<MainSettings>();

            return Ok(mainSettings);
        }

        [HttpPost]
        [Route("Contact/")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> AddContact([FromBody]string userId)
        {
            string accessToken = await HttpContext.GetToken();
            var session = await _sessionService.GetSession(accessToken);
            if (session == null)
                return BadRequest(new { message = "Session expired. Please login again." });

            var newContact = await _userRepository.FindUser(userId);
            if (newContact == null)
                return BadRequest(new { message = $"Contact doesn't exist: {userId}" });

            var contactList = await _userRepository.GetContacts(session.UserId);
            if (contactList.Contains(newContact.UserName))
                return BadRequest(new { message = $"Contact already in contact list: {userId}" });

            var added = await _userRepository.AddContact(session.UserId, newContact.UserName);

            await _log.InfoAsync($"New Contact Added: [{newContact.UserName}]", context: session.UserId);
            return Ok(added);
        }

        [HttpDelete]
        [Route("Contact/")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteContact([FromBody]string userId)
        {
            string accessToken = await HttpContext.GetToken();
            var session = await _sessionService.GetSession(accessToken);
            if (session == null)
                return BadRequest(new { message = "Session expired. Please login again." });

            var removedContact = await _userRepository.FindUser(userId);
            if (removedContact == null)
                return BadRequest(new { message = $"Contact doesn't exist: {userId}" });

            var contactList = await _userRepository.GetContacts(session.UserId);
            if (!contactList.Contains(removedContact.UserName))
                return BadRequest(new { message = $"Contact not in contact list: {userId}" });

            var removed = await _userRepository.DeleteContact(session.UserId, removedContact.UserName);
            if (removed)
            {
                await _log.InfoAsync($"Contact Removed: [{removedContact.UserName}]", context: session.UserId);
                return Ok(removed);
            }
            else
                return BadRequest(new { message = $"Failed to remove contact from contact list: {userId}" });
        }

        [HttpGet]
        [Route("Contact/")]
        [ProducesResponseType(typeof(IEnumerable<string>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetContacts()
        {   
            string accessToken = await HttpContext.GetToken();
            var session = await _sessionService.GetSession(accessToken);
            if (session == null)
                return BadRequest(new { message = "Session expired. Please login again." });

            var contactList = await _userRepository.GetContacts(session.UserId);
            return Ok(contactList);
        }

    }
}
