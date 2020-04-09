using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Contracts.Requests;
using NBsoft.Wordzz.Contracts.Results;
using NBsoft.Wordzz.Core.Repositories;
using NBsoft.Wordzz.Core.Services;

namespace NBsoft.Wordzz.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class LexiconController : ControllerBase
    {
        private readonly IWordRepository _wordRepository;
        private readonly ISessionService _sessionService;

        public LexiconController(IWordRepository wordRepository, ISessionService sessionService)
        {
            _wordRepository = wordRepository;
            _sessionService = sessionService;
        }

        [HttpPost]
        [Route("Dictionary/")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AddDictionary([FromBody]DictionaryRequest request)
        {
            var result = await HttpContext.AuthenticateAsync();
            string accessToken = result.Properties.Items[".Token.access_token"];
            var session = await _sessionService.GetSession(accessToken);
            if (session == null)
                return BadRequest(new { message = "Session expired. Please login again." });

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
                return Ok(isOK);
            else
                return BadRequest(new { message = "Dictionary creation failed" });
        }

        [HttpGet]
        [Route("Dictionary/")]
        [ProducesResponseType(typeof(FileResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetDictionary([FromQuery]string language)
        {
            var result = await HttpContext.AuthenticateAsync();
            string accessToken = result.Properties.Items[".Token.access_token"];
            var session = await _sessionService.GetSession(accessToken);
            if (session == null)
                return BadRequest(new { message = "Session expired. Please login again." });

            var lexicon = await _wordRepository.GetDictionary(language);
            var words = await _wordRepository.ListWords(lexicon);

            var fileBytes = Core.Compression.Zip.Compress(string.Join(",", words));

            return File(fileBytes, MediaTypeNames.Application.Octet, $"{lexicon.Language}.zip");
            

        }
    }
}
