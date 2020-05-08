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
    public class BoardController : ControllerBase
    {
        private readonly ILogger log;
        private readonly IBoardRepository boardRepository;
        private readonly ISessionService sessionService;

        public BoardController(ILogger log, IBoardRepository boardRepository, ISessionService sessionService)
        {
            this.log = log;
            this.boardRepository = boardRepository;
            this.sessionService = sessionService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<IBoard>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> List()
        {

            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });

            try
            {
                var res = await boardRepository.List();
                return Ok(res);
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in boardRepository.List()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }

        }
                
        [HttpPost]
        [ProducesResponseType(typeof(IBoard), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Add([FromBody] Board board)
        {

            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });
            if (session.UserId != Constants.AdminUser)
                return BadRequest(new { message = "Not authorized" });

            try
            {
                var res = await boardRepository.Add(board);
                return Ok(res);
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in boardRepository.Add()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }

        [HttpPut]
        [ProducesResponseType(typeof(IBoard), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Update([FromBody] Board board)
        {

            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });
            if (session.UserId != Constants.AdminUser)
                return BadRequest(new { message = "Not authorized" });

            try
            {
                var res = await boardRepository.Update(board);
                return Ok(res);
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in boardRepository.Update()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }

        [HttpDelete, Route("{boardId}")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Delete(int boardId)
        {

            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });
            if (session.UserId != Constants.AdminUser)
                return BadRequest(new { message = "Not authorized" });

            try
            {
                var res = await boardRepository.Delete(boardId);
                return Ok(res);
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in boardRepository.Delete()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }

        [HttpGet, Route("{boardId}")]
        [ProducesResponseType(typeof(IBoard), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(Nullable), (int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Get(int boardId)
        {

            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });

            try
            {
                var res = await boardRepository.Get(boardId);
                return Ok(res);
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in boardRepository.Get()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }
    }
}
