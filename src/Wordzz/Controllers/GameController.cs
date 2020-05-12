using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NBsoft.Logs;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Contracts.Requests;
using NBsoft.Wordzz.Core.Models;
using NBsoft.Wordzz.Core.Repositories;
using NBsoft.Wordzz.Core.Services;
using NBsoft.Wordzz.Extensions;
using NBsoft.Wordzz.Hubs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Wordzz.Contracts.Requests;

namespace NBsoft.Wordzz.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class GameController: ControllerBase
    {
        private readonly ILogger log;
        private readonly IBoardRepository boardRepository;
        private readonly IUserRepository userRepository;
        private readonly ISessionService sessionService;
        private readonly IGameService gameService;
        private readonly IGameQueueService gameQueueService;
        private readonly ILexiconService lexiconService;
        private readonly IHubContext<GameHub, IGameClient> gameHubContext;



        public GameController(ILogger log, IBoardRepository boardRepository, IUserRepository userRepository,
            ISessionService sessionService, IGameService gameService, IGameQueueService gameQueueService, ILexiconService lexiconService,
            IHubContext<GameHub, IGameClient> gameHubContext)
        {
            this.log = log;
            this.boardRepository = boardRepository;
            this.userRepository = userRepository;

            this.sessionService = sessionService;
            this.gameService = gameService;
            this.gameQueueService = gameQueueService;
            this.lexiconService = lexiconService;

            this.gameHubContext = gameHubContext;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<IGame>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ActiveGamesList()
        {

            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });

            try
            {
                var res = gameService.GetAllActiveGames();
                return Ok(res);
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in boardRepository.List()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }

        }

        [HttpGet, Route("{userName}")]
        [ProducesResponseType(typeof(IEnumerable<IGame>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(Nullable), (int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ActiveGames(string userName)
        {

            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });

            try
            {
                Console.WriteLine($"{DateTime.UtcNow} - ActiveGames: {userName}");
                var res = gameService.GetActiveGames(userName);
                return Ok(res);
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in boardRepository.Get()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }

        [HttpGet, Route("Queue")]
        [ProducesResponseType(typeof(IEnumerable<IGameQueue>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]        
        public async Task<IActionResult> GetQueueList()
        {
            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });

            try
            {
                var res = gameQueueService.AllQueues();
                return Ok(res);
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in userRepository.GetDetails()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }

        [HttpGet, Route("Queue/{userName}")]
        [ProducesResponseType(typeof(IEnumerable<IGameQueue>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetQueues(string userName)
        {
            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });

            try
            {
                var res = gameQueueService.GetQueues(userName);
                return Ok(res);
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in GetQueues()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }

        [HttpPost, Route("Queue")]
        [ProducesResponseType(typeof(IGameQueue), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> QueueGame([FromBody] QueueGameRequest request)
        {
            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });

            try
            {
                if (request.BoardId == 0)
                {
                    var b = await GetDefaultBoard();
                    request.BoardId = b.Id;
                }

                // Validate language and board
                var lexicon = await lexiconService.GetDictionary(request.Language);
                if (lexicon == null)
                    return null;
                var board = await GetBoard(request.BoardId);
                if (board == null)
                    return null;

                var queue = gameQueueService.QueueGame(lexicon.Language, board.Id, session.UserId);

                return Ok(queue);
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in QueueGame()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }

        [HttpDelete, Route("Queue/{queueId}")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> RemoveQueue(string queueId)
        {
            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });
            try
            {
                var result = gameQueueService.RemoveQueue(queueId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in QueueGame()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }

        [HttpGet, Route("Challenge")]
        [ProducesResponseType(typeof(IEnumerable<IGameChallenge>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetChallengesSent()
        {
            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });

            try
            {
                var queues = gameQueueService.GetSentChallenges(session.UserId);
                var challenges = queues.Select(q => new GameChallenge { Id = q.Id, Language = q.Language, BoardId = q.BoardId, Origin = q.Player1, Destination = q.Player2 });
                return Ok(challenges);
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in GetQueues()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }
        
        [HttpPost, Route("Challenge")]
        [ProducesResponseType(typeof(IGameChallenge), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ChallengeGame([FromBody] ChallengeGameRequest request)
        {
            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });

            try
            {
                if (request.BoardId== 0)
                {
                    var b = await GetDefaultBoard();
                    request.BoardId = b.Id;
                }

                var lexicon = await lexiconService.GetDictionary(request.Language);
                if (lexicon == null)
                    throw new ApplicationException("Invalid Language");
                var board = await GetBoard(request.BoardId);
                if (board == null)
                    throw new ApplicationException("Invalid Board");
                var player2 = await userRepository.Get(request.Challenged);
                if (player2 == null)
                    throw new ApplicationException("Invalid Opposer");

                var challenge = await gameService.ChallengeGame(lexicon.Language, board.Id, session.UserId, request.Challenged);

                var challenged = ClientHandler.FindByUserName(request.Challenged);
                if (challenged != null)
                    await gameHubContext.Clients.Client(challenged.ConnectionId).NewChallenge(challenge);
                return Ok(challenge);
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in QueueGame()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }

        
        [HttpDelete, Route("Challenge/{challengeId}")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> RemoveChallenge(string challengeId)
        {
            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });
            try
            {
                var challenge = gameQueueService.GetSentChallenges(session.UserId).SingleOrDefault(q => q.Id == challengeId);
                var result = gameQueueService.RemoveQueue(challengeId);
                if (result && challenge != null)
                {
                    var challenged = ClientHandler.FindByUserName(challenge.Player2);
                    if (challenged != null)
                        await gameHubContext.Clients.Client(challenged.ConnectionId).ChallengeCanceled(challengeId);
                }                    
                return Ok(result);
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in QueueGame()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }

        [HttpGet, Route("Challenge/Received")]
        [ProducesResponseType(typeof(IEnumerable<IGameChallenge>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetChallengesReceived()
        {
            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });

            try
            {
                var queues = gameQueueService.GetReceivedChallenges(session.UserId);
                var challenges = queues.Select(q => new GameChallenge { Id = q.Id, Language = q.Language, BoardId = q.BoardId, Origin = q.Player1, Destination = q.Player2 }); 
                return Ok(challenges);
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in GetQueues()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }

        [HttpPost, Route("Challenge/Received")]
        [ProducesResponseType(typeof(IGame), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(Nullable), (int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AcceptChallenge([FromBody] ChallengeAcceptRequest request)
        {
            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });

            try
            {
                var queue = gameQueueService.GetQueue(request.ChallengeId);
                if (queue == null)
                    throw new ApplicationException("Invalid Challenge Id");
                var game = await gameService.AcceptChallenge(request.ChallengeId, request.Accept);

                var challenger = ClientHandler.FindByUserName(queue.Player1);
                if (challenger != null)
                    await gameHubContext.Clients.Client(challenger.ConnectionId).ChallengeAccepted(queue.Id, request.Accept, game?.Id);

                if (game != null)
                    return Ok(game);
                return NoContent();
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in QueueGame()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }



        private async Task<IBoard> GetBoard(int boardId)
        {
            var boards = await boardRepository.List();
            var selected = boards.SingleOrDefault(b => b.Id == boardId);
            if (selected == null)
                throw new Exception($"Ivalid board Id {boardId}");
            return await boardRepository.Get(selected.Id);
        }
        private async Task<IBoard> GetDefaultBoard()
        {
            var boards = await boardRepository.List();
            var selected = boards.SingleOrDefault(b => b.Name == "Standard");
            return await boardRepository.Get(selected.Id);
        }
    }
}
