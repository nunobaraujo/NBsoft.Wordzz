using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NBsoft.Logs;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Core.Models;
using NBsoft.Wordzz.Core.Repositories;
using NBsoft.Wordzz.Core.Services;
using NBsoft.Wordzz.Entities;
using NBsoft.Wordzz.Extensions;
using NBsoft.Wordzz.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Services
{
    internal class SessionService : ISessionService
    {
        public const int SessionMaxAge = 86400000;     // 1 day = 86400000 milliseconds
        public const int SessionTimeout = 14400000;     // 4 hours = 14400000 milliseconds (front office can be open all morning without timout)
                
        private readonly ILogger _log;
        private readonly IUserRepository _userRepository;
        private readonly ISessionRepository _sessionRepository;
        private readonly WordzzSettings _settings;

        private string IV;
        private Timer sessionCheckTimer;
        private bool isDisposing = false;

        public SessionService(ILogger log, ISessionRepository sessionRepository, IUserRepository userRepository, IOptions<WordzzSettings> settings)
        {
            _log = log;
            _userRepository = userRepository;
            _sessionRepository = sessionRepository;
            _settings = settings.Value;

            IV = _settings.EncryptionKey.Replace("-", "").Substring(16);

            sessionCheckTimer = new Timer(new TimerCallback(SessionCheckTimerCallback));
            sessionCheckTimer.Change(5 * 1000, -1);
        }
        ~SessionService() { isDisposing = true; }

        public async Task<IEnumerable<ISession>> GetAll()
        {
            var dbSessions = await _sessionRepository.List();

            var validSessions = new List<ISession>();
            foreach (var session in dbSessions)
            {
                var isValid = await GetSession(session.SessionToken);
                if (isValid != null)
                    validSessions.Add(isValid);
            }
            return validSessions;
        }

        public async Task<ISession> GetSession(string sessionToken)
        {

            if (sessionToken == null)
                return null;

            var session = await _sessionRepository.Get(sessionToken);
            if (session == null)
                return null;

            // Session expiration time hit
            if (DateTime.UtcNow > session.Registered.AddMilliseconds(SessionMaxAge))
            {
                await _sessionRepository.Remove(sessionToken);                
                await _log.InfoAsync($"Session expired: [{session.SessionToken}]", context: session.UserId);
                return null;
            }

            // Session timeout
            if (DateTime.UtcNow > session.LastAction.AddMilliseconds(SessionTimeout))
            {
                await _sessionRepository.Remove(sessionToken);                
                await _log.InfoAsync($"Session timeout: [{session.SessionToken}]", context: session.UserId);
                return null;
            }
            
            return session;
        }

        public async Task<ISession> LogIn(string userName, string password, string userInfo)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                throw new ArgumentException($"{nameof(userName)} and {nameof(password)} cannot be empty.");


            IUser user = null;
            // Admin Access
            if (userName == Constants.AdminUser && password == Constants.AdminPassword)
                user = (await _userRepository.Get(Constants.AdminUser));
            else
                user = await _userRepository.Auth(userName, password);

            if (user == null)
                return null;

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_settings.EncryptionKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.UserName)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var sessionToken = tokenHandler.WriteToken(token);

            return await CreateNewSession(user.UserName, userInfo, sessionToken);
        }

        public async Task LogOut(string sessionToken)
        {
            if (string.IsNullOrEmpty(sessionToken))
                throw new ArgumentException("Value cannot be empty", nameof(sessionToken));
            try
            {
                var session = await GetSession(sessionToken);
                if (session != null)
                {
                    await _sessionRepository.Remove(sessionToken);
                    await _log.InfoAsync($"Session Terminated: [{session.SessionToken}]", context: session.UserId);
                }
            }
            catch (Exception ex)
            {   
                await _log.ErrorAsync(ex.Message, ex, context: sessionToken);
                throw;
            }
        }
        
        private async Task<ISession> CreateNewSession(string userId, string userInfo, string token)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("Value cannot be empty", nameof(userId));

            try
            {
                var sessionStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, DateTime.UtcNow.Second);
                var newSession = new Session
                {
                    UserId = userId,
                    UserInfo = userInfo,
                    Registered = sessionStart,
                    LastAction = sessionStart,
                    SessionToken = token
                };
                await _sessionRepository.New(newSession);                
                await _log.InfoAsync($"New session: [{newSession.SessionToken}]", context: newSession.UserId);
                return newSession;
            }
            catch (Exception ex)
            {   
                await _log.ErrorAsync(ex.Message, ex, context: userId);
                throw;
            }
        }

        private void SessionCheckTimerCallback(object status)
        {
            sessionCheckTimer.Change(-1, -1);

            var getActiveTask = GetAll();
            try { getActiveTask.Wait(); }
            catch { }
            
            if (!isDisposing)
                sessionCheckTimer.Change(60 * 1000, -1);
            else
                sessionCheckTimer.Dispose();

        }


    }
}
