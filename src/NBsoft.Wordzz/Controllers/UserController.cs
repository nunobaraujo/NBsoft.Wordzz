using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NBsoft.Logs;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Contracts.Requests;
using NBsoft.Wordzz.Contracts.Settings;
using NBsoft.Wordzz.Core.Models;
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
        private readonly IUserRepository userRepository;
        private readonly ISessionService sessionService;
        private readonly ILogger log;

        public UserController(IUserRepository userRepository, ISessionService sessionService, ILogger log)
        {            
            this.userRepository = userRepository;
            this.sessionService = sessionService;
            this.log = log;
        }

        [AllowAnonymous]
        [HttpPost]        
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(IUser), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Add([FromBody]LogInRequest request)
        {
            var user = new User {
                CreationDate = DateTime.UtcNow,
                Deleted = false,
                UserName = request.UserName,
                PasswordHash = "",
                Salt = ""
            };
            
            try
            {
                var newUser = await userRepository.Add(user, request.Email);
                await userRepository.SetPassword(request.UserName, request.Password);

                await log.InfoAsync($"New User Created: [{newUser.UserName}]");
                return Ok(newUser);
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in userRepository.Add()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]        
        [ProducesResponseType(typeof(IUser), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Get()
        {
            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });

            try
            {
                var user = await userRepository.Get(session.UserId);
                return Ok(user.Sanitize());
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in userRepository.Add()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }

        [HttpGet, Route("Details")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(IUser), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetDetails()
        {
            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });

            try
            {
                var details = await userRepository.GetDetails(session.UserId);
                return Ok(details);
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in userRepository.GetDetails()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }

        [HttpPut, Route("Details")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(IUserDetails), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateDetails([FromBody] UserDetails details)
        {
            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });
            try
            {
                if (details == null)
                    throw new ArgumentNullException("Details cannot be null");
                if (details.UserName != session.UserId)
                    throw new NotSupportedException("You are not allowed to change other user's details");

                var updated = await userRepository.UpdateDetails(details);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in userRepository.UpdateDetails()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("Contact")]
        [ProducesResponseType(typeof(IEnumerable<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetContacts()
        {
            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });

            try
            {
                var contactList = await userRepository.GetContacts(session.UserId);
                return Ok(contactList);
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in userRepository.GetContacts()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("Contact")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> AddContact([FromBody]ContactRequest request)
        {
            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });

            var newContact = await userRepository.FindUser(request.UserName);
            if (newContact == null)
                return BadRequest(new { message = $"Contact doesn't exist: {request.UserName}" });

            var contactList = await userRepository.GetContacts(session.UserId);
            if (contactList.Contains(newContact.UserName))
                return BadRequest(new { message = $"Contact already in contact list: {request.UserName}" });

            try
            {
                var added = await userRepository.AddContact(session.UserId, newContact.UserName);
                await log.InfoAsync($"New Contact Added: [{newContact.UserName}]", context: session.UserId);
                return Ok(true);
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in userRepository.AddContact()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }

        [HttpDelete]
        [Route("Contact")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteContact([FromQuery]ContactRequest request)
        {
            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });

            var removedContact = await userRepository.FindUser(request.UserName);
            if (removedContact == null)
                return BadRequest(new { message = $"Contact doesn't exist: {request.UserName}" });

            var contactList = await userRepository.GetContacts(session.UserId);
            if (!contactList.Contains(removedContact.UserName))
                return BadRequest(new { message = $"Contact not in contact list: {request.UserName}" });

            try
            {
                var removed = await userRepository.DeleteContact(session.UserId, removedContact.UserName);
                if (removed)
                {
                    await log.InfoAsync($"Contact Removed: [{removedContact.UserName}]", context: session.UserId);
                    return Ok(removed);
                }
                else
                    return BadRequest(new { message = $"Failed to remove contact from contact list: {request.UserName}" });
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in userRepository.DeleteContact()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }
                

        [HttpGet, Route("Settings/Main")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(MainSettings), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetMainSettings()
        {
            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });
            try
            {
                var settings = await userRepository.GetSettings(session.UserId);
                var mainSettings = settings?.MainSettings?.FromJson<MainSettings>();
                return Ok(mainSettings);
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in userRepository.GetSettings()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }

        [HttpPut, Route("Settings/Main")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(MainSettings), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateMainSettings([FromBody] MainSettings settings)
        {
            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });
            try
            {
                if (settings == null)
                    throw new ArgumentNullException("Settings cannot be null");
                if (settings.UserId != session.UserId)
                    throw new NotSupportedException("You are not allowed to change other user's settings");

                var existing = (await userRepository.GetSettings(session.UserId)).ToDto<UserSettings>();
                existing.MainSettings = settings.ToJson();
                var updated = await userRepository.UpdateSettings(existing);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in userRepository.GetSettings()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }

        [HttpGet, Route("Settings/Android")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(AndroidSettings), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAndroidSettings()
        {
            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });
            try
            {
                var settings = await userRepository.GetSettings(session.UserId);
                var androidSettings = settings?.AndroidSettings?.FromJson<AndroidSettings>();
                return Ok(androidSettings);
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in userRepository.GetSettings()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }

        [HttpPut, Route("Settings/Android")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(AndroidSettings), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateAndroidSettings([FromBody] AndroidSettings settings)
        {
            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });
            try
            {
                if (settings == null)
                    throw new ArgumentNullException("Settings cannot be null");
                if (settings.UserId != session.UserId)
                    throw new NotSupportedException("You are not allowed to change other user's settings");

                var existing = (await userRepository.GetSettings(session.UserId)).ToDto<UserSettings>();
                existing.AndroidSettings = settings.ToJson();
                var updated = await userRepository.UpdateSettings(existing);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in userRepository.GetSettings()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }

        [HttpGet, Route("Settings/Ios")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(IOSSettings), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetIosSettings()
        {
            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });
            try
            {
                var settings = await userRepository.GetSettings(session.UserId);
                var iOSSettings = settings?.IOSSettings?.FromJson<IOSSettings>();
                return Ok(iOSSettings);
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in userRepository.GetSettings()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }

        [HttpPut, Route("Settings/Ios")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(AndroidSettings), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateIosSettings([FromBody] IOSSettings settings)
        {
            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });
            try
            {
                if (settings == null)
                    throw new ArgumentNullException("Settings cannot be null");
                if (settings.UserId != session.UserId)
                    throw new NotSupportedException("You are not allowed to change other user's settings");

                var existing = (await userRepository.GetSettings(session.UserId)).ToDto<UserSettings>();
                existing.IOSSettings = settings.ToJson();
                var updated = await userRepository.UpdateSettings(existing);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in userRepository.GetSettings()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }

        [HttpGet, Route("Settings/Windows")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(IOSSettings), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetWindowsSettings()
        {
            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });
            try
            {
                var settings = await userRepository.GetSettings(session.UserId);
                var windowsSettings = settings?.WindowsSettings?.FromJson<WindowsSettings>();
                return Ok(windowsSettings);
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in userRepository.GetSettings()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }

        [HttpPut, Route("Settings/Windows")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(AndroidSettings), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateWindowsSettings([FromBody] WindowsSettings settings)
        {
            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });
            try
            {
                if (settings == null)
                    throw new ArgumentNullException("Settings cannot be null");
                if (settings.UserId != session.UserId)
                    throw new NotSupportedException("You are not allowed to change other user's settings");

                var existing = (await userRepository.GetSettings(session.UserId)).ToDto<UserSettings>();
                existing.WindowsSettings = settings.ToJson();
                var updated = await userRepository.UpdateSettings(existing);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in userRepository.GetSettings()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }
       

    }
}
