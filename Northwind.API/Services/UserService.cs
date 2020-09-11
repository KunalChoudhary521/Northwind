using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Northwind.API.Repositories;
using Northwind.Data.Entities;

namespace Northwind.API.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly ICryptoService _cryptoService;
        private readonly ILogger<UserService> _logger;

        public UserService(IRepository<User> userRepository,
                           ICryptoService cryptoService,
                           ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _cryptoService = cryptoService;
            _logger = logger;
        }

        public async Task<ICollection<User>> GetAll()
        {
            _logger.LogInformation("Retrieving all users");
            return await _userRepository.FindAll().Include(user => user.RefreshToken).ToArrayAsync();
        }

        public async Task<User> GetById(int userId)
        {
            _logger.LogInformation($"Retrieving users with id: {userId}");
            return await QueryWithRefreshToken(user => user.UserId == userId).FirstOrDefaultAsync();
        }

        public async Task<User> GetByUserName(string userName)
        {
            _logger.LogInformation($"Retrieving user with username: {userName}");
            return await QueryWithRefreshToken(u => u.UserName == userName).FirstOrDefaultAsync();
        }

        public void Add(User user, string password)
        {
            _logger.LogInformation($"Adding user with username: {user.UserName}");

            _cryptoService.EncryptPassword(password, out var salt, out var hash);
            user.PasswordSalt = salt;
            user.PasswordHash = hash;

            _userRepository.Add(user);
        }

        public void Delete(User user)
        {
            _logger.LogInformation($"Deleting user with id: {user.UserId}");
            _userRepository.Delete(user);
        }

        public async Task<bool> IsSavedToDb()
        {
            return await _userRepository.SaveChangesAsync();
        }

        private IQueryable<User> QueryWithRefreshToken(Expression<Func<User, bool>> expression)
        {
            return _userRepository.FindByCondition(expression).Include(user => user.RefreshToken);
        }
    }
}