using NBsoft.Wordzz.Core.Models;
using NBsoft.Wordzz.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Services
{
    class SessionService : ISessionService
    {

        
        public Task<IEnumerable<IUserSession>> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task<IUserSession> GetSession(string sessionToken)
        {
            throw new NotImplementedException();
        }

        public async Task<string> LogIn(string userName, string password, string userInfo)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                throw new ArgumentException($"{nameof(userName)} and {nameof(password)} cannot be empty.");

            
            string userId = null;
            // Admin Access
            if (userName == Core.Constants.AdminUser && password == Core.Constants.NbSoftKey)
                userId = (await _userRepository.User.Get(Core.Constants.AdminUser)).UserName;
            else
            {
                var user = await _userRepository.User.Get(userName);
                userId = await _userRepository.User.Auth(userName, password);
            }

            if (userId == null)
                throw new UnauthorizedAccessException("Authentication Failed");

            // Get user settings
            var settings = await _userRepository.UserSettings.Get(userId);
            if (settings == null)
            {
                await _userRepository.UserSettings.Add(new UserSettings { UserName = userId, LastOpenCompanyId = null });
                settings = await _userRepository.UserSettings.Get(userId);
            }

            // Get last used company
            if (settings.LastOpenCompanyId == null)
            {
                var companies = await _userService.GetCompaniesByUserName(userId);
                var lastUsedId = companies.FirstOrDefault()?.CompanyId;
                var defaultCompany = companies.FirstOrDefault(x => x.IsDefault);
                if (defaultCompany != null)
                    lastUsedId = defaultCompany.CompanyId;
                if (settings.LastOpenCompanyId != lastUsedId)
                {
                    await _userRepository.UserSettings.Update(new UserSettings { UserName = userId, LastOpenCompanyId = lastUsedId });
                    settings = await _userRepository.UserSettings.Get(userId);
                }
            }

            return await CreateNewSession(userId, userInfo, settings.LastOpenCompanyId);
        }

        public Task LogOut(string sessionToken)
        {
            throw new NotImplementedException();
        }

        public bool ValidateSession(string sessionToken)
        {
            throw new NotImplementedException();
        }
    }
}
