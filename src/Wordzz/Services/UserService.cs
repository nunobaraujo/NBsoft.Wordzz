using NBsoft.Logs;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Core.Models;
using NBsoft.Wordzz.Core.Repositories;
using NBsoft.Wordzz.Core.Services;
using NBsoft.Wordzz.Entities;
using System;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;
        private readonly ILogger log;

        public UserService(IUserRepository userRepository, ILogger log)
        {
            this.userRepository = userRepository;
            this.log = log;
        }

        public async Task<IUser> AddUser(string userName, string userEmail, string password)
        {
            var user = new User
            {
                CreationDate = DateTime.UtcNow,
                Deleted = false,
                UserName = userName,
                PasswordHash = "",
                Salt = ""
            };
            var newUser = await userRepository.Add(user, userEmail);
            await userRepository.SetPassword(userName, password);

            await log.InfoAsync($"New User Created: [{newUser.UserName}]");
            return newUser;
        }

        public Task<bool> RequestPasswordChange(string userEmail)
        {
            // Send EMAIL to email with the link
            throw new System.NotImplementedException();
        }
    }
}
