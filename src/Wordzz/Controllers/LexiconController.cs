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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class LexiconController : ControllerBase
    {
        private readonly IWordRepository wordRepository;
        private readonly ISessionService sessionService;
        private readonly ILexiconService lexiconService;
        private readonly ILogger log;

        public LexiconController(IWordRepository wordRepository, ISessionService sessionService, ILexiconService lexiconService, ILogger log)
        {
            this.wordRepository = wordRepository;
            this.sessionService = sessionService;
            this.lexiconService = lexiconService;
            this.log = log;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ILexicon>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> List()
        {
            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });

            try
            {
                var dics = await wordRepository.ListDictionaries();
                return Ok(dics);
            }
            catch(Exception ex)
            {
                await log.ErrorAsync("Error in wordRepository.ListDictionaries()", ex);
                return BadRequest(new { title = ex.GetType().ToString(),  details = ex.StackTrace , message = ex.Message });
            }
        }

        [HttpPost]        
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Add([FromBody]DictionaryRequest request)
        {
            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });

            if (session.UserId != Constants.AdminUser)
                return BadRequest(new { message = "Not authorized" });

            try
            {
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

                await log.InfoAsync($"Creating new Lexicon: [{lexicon.Language}]", context: session.UserId);
                var isOK = await wordRepository.AddDictionary(lexicon, words);

                if (isOK)
                {
                    await log.InfoAsync($"New Lexicon Created: [{lexicon.Language}]", context: session.UserId);
                    return Ok(isOK);
                }
                throw new ApplicationException("Lexicon creation failed");
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in wordRepository.AddDictionary()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }

        [HttpDelete, Route("{language}")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Delete(string language)
        {
            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });

            if (session.UserId != Constants.AdminUser)
                return BadRequest(new { message = "Not authorized" });

            try
            {
                var result = await wordRepository.DeleteDictionary(language);
                return Ok(result);


            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in wordRepository.DeleteDictionary()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }

        [HttpGet, Route("{language}")]
        [ProducesResponseType(typeof(ILexicon), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(Nullable), (int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Get(string language)
        {
            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });
            try
            {
                var res = await wordRepository.GetDictionary(language);
                return Ok(res);
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in wordRepository.GetDictionary()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }

        [HttpGet, Route("{language}/{word}")]
        [ProducesResponseType(typeof(ILexicon), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(Nullable), (int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetWord(string language, string word)
        {
            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });
            try
            {
                var lexicon = await lexiconService.GetDictionary(language);
                if (lexicon == null)
                    return NoContent();

                var result = await lexiconService.GetWordInfo(lexicon.Language, word);
                return Ok(result);
            }
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in wordRepository.GetDictionary()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }

        [HttpGet, Route("File/{language}")]
        [ProducesResponseType(typeof(FileResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetFile(string language)
        {
            string accessToken = await HttpContext.GetToken();
            var session = await sessionService.GetSession(accessToken);
            if (session == null)
                return Unauthorized(new { message = "Session expired. Please login again." });

            if (session.UserId != Constants.AdminUser)
                return BadRequest(new { message = "Not authorized" });

            try
            {
                var words = await wordRepository.ListWords(language);
                var fileBytes = Core.Compression.Zip.Compress(string.Join(",", words));
                return File(fileBytes, MediaTypeNames.Application.Octet, $"{language}.zip");
            }
             
            catch (Exception ex)
            {
                await log.ErrorAsync("Error in wordRepository.ListWords()", ex);
                return BadRequest(new { title = ex.GetType().ToString(), details = ex.StackTrace, message = ex.Message });
            }
        }
    }
}
