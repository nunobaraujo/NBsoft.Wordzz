using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NBsoft.Logs;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Contracts.Requests;
using NBsoft.Wordzz.Core.Repositories;
using NBsoft.Wordzz.Core.Services;
using NBsoft.Wordzz.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class StatsController: ControllerBase
    {
        private readonly ILogger log;
        private readonly IStatsRepository statsRepository;
        private readonly ISessionService sessionService;

        public StatsController(ILogger log, IStatsRepository statsRepository, ISessionService sessionService)
        {
            this.log = log;
            this.statsRepository = statsRepository;
            this.sessionService = sessionService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<IUserStats>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> List()
        {

            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });

            try
            {
                var res = await statsRepository.GetHighScores();
                return Ok(res);
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in statsRepository.GetHighScores()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }

        }

        [HttpGet, Route("{userName}")]
        [ProducesResponseType(typeof(IUserStats), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(Nullable), (int)HttpStatusCode.NoContent)]        
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Get(string userName)
        {

            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });

            try
            {
                var res = await statsRepository.Get(userName);
                return Ok(res);
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in statsRepository.Get()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }

        }

    }
}
