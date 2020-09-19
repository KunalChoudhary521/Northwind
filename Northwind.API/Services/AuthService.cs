using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Northwind.API.Helpers;
using Northwind.API.Repositories;
using Northwind.Data.Entities;

namespace Northwind.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _userRepository;
        private readonly ICryptoService _cryptoService;
        private readonly AuthSettings _authSettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IRepository<User> userRepository,
                           ICryptoService cryptoService,
                           IOptions<AuthSettings> authSettings,
                           ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _cryptoService = cryptoService;
            _authSettings = authSettings.Value;
            _logger = logger;
        }

        public async Task<User> GetByCredentials(string userName, string password)
        {
            _logger.LogInformation($"Retrieving user with username: {userName}");
            var user = await QueryWithRefreshToken(u => u.UserName == userName).FirstOrDefaultAsync();

            if (user == null || !_cryptoService.IsPasswordCorrect(password, user.PasswordSalt, user.PasswordHash))
                return null;

            return user;
        }

        public async Task<User> GetByRefreshToken(string refreshToken)
        {
            _logger.LogInformation("Retrieving user with refresh token");
            return await QueryWithRefreshToken(u => u.RefreshToken.Value == refreshToken).FirstOrDefaultAsync();
        }

        public User CreateCredentials(User user)
        {
            _logger.LogInformation($"Generating credentials for user: {user.UserName}");
            return GenerateTokens(user);
        }

        public User RefreshCredentials(User user)
        {
            _logger.LogInformation($"Refreshing credentials for user: {user.UserName}");
            return GenerateTokens(user);
        }

        public void RevokeCredentials(User user)
        {
            _logger.LogInformation($"Revoking credentials for user: {user.UserName}");

            user.AccessToken = null;
            user.RefreshToken.Value = null;
            user.RefreshToken.ExpiryDate = null;
            user.RefreshToken.RevokeDate = DateTimeOffset.UtcNow;

            _userRepository.Update(user);
        }

        public async Task<bool> IsSavedToDb()
        {
            return await _userRepository.SaveChangesAsync();
        }

        private string GenerateAccessToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = Encoding.UTF8.GetBytes(_authSettings.SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new []
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserIdentifier.ToString()),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                }),
                Expires = DateTimeOffset.UtcNow.Add(new TimeSpan(_authSettings.ExpiryDuration)).UtcDateTime,
                Audience = _authSettings.ValidAudience,
                Issuer = _authSettings.ValidIssuer,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey),
                                                            SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private RefreshToken GenerateRefreshToken()
        {
            using var rng = RandomNumberGenerator.Create();
            using var crypt = new SHA256CryptoServiceProvider();
            var refreshBytes = new byte[64];
            rng.GetBytes(refreshBytes);

            var refreshHash = crypt.ComputeHash(refreshBytes);
            return new RefreshToken
            {
                Value = Convert.ToBase64String(refreshHash),
                CreateDate = DateTimeOffset.UtcNow,
                ExpiryDate = DateTimeOffset.UtcNow.Add(new TimeSpan(_authSettings.ExpiryDuration * 2)),
                RevokeDate = null
            };
        }

        private User GenerateTokens(User user)
        {
            user.AccessToken = GenerateAccessToken(user);
            user.RefreshToken = GenerateRefreshToken();

            _userRepository.Update(user);
            return user;
        }

        private IQueryable<User> QueryWithRefreshToken(Expression<Func<User, bool>> expression)
        {
            return _userRepository.FindByCondition(expression).Include(user => user.RefreshToken);
        }
    }
}