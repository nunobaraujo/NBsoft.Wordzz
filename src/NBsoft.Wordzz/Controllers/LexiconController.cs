using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NBsoft.Logs;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Contracts.Requests;
using NBsoft.Wordzz.Core.Repositories;
using NBsoft.Wordzz.Core.Services;
using NBsoft.Wordzz.Extensions;
using System;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class LexiconController : ControllerBase
    {
        private readonly IWordRepository _wordRepository;
        private readonly ISessionService _sessionService;
        private readonly ILogger _log;

        public LexiconController(IWordRepository wordRepository, ISessionService sessionService, ILogger log)
        {
            _wordRepository = wordRepository;
            _sessionService = sessionService;
            _log = log;
        }

        [HttpPost]
        [Route("Dictionary/")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AddDictionary([FromBody]DictionaryRequest request)
        {
            string accessToken = await HttpContext.GetToken();
            var session = await _sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });

            if (session.UserId != Constants.AdminUser)
                return BadRequest(new { message = "Not authorized" });

            var lexicon = new Lexicon
            {
                CreationDate = DateTime.Now,
                Language = request.Language,
                Description = request.Description,
            };

            var words = request.Words.Select(w => new Word 
            { 
                Language = lexicon.Language,
                Name = w
            });

            var isOK = await _wordRepository.AddDictionary(lexicon, words);

            if (isOK)
            {
                await _log.InfoAsync($"New Lexicon Created: [{lexicon.Language}]", context: session.UserId);
                
                return Ok(isOK); 
            }
            else
                return BadRequest(new { message = "Dictionary creation failed" });
        }

        [HttpGet]
        [Route("Dictionary/")]
        [ProducesResponseType(typeof(FileResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetDictionary([FromQuery]string language)
        {
            string accessToken = await HttpContext.GetToken();
            var session = await _sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });
                        
            var words = await _wordRepository.ListWords(language);

            var fileBytes = Core.Compression.Zip.Compress(string.Join(",", words));

            return File(fileBytes, MediaTypeNames.Application.Octet, $"{language}.zip");
            

        }
    }
}
