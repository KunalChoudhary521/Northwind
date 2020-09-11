using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using Moq;
using Northwind.API.Helpers;
using Northwind.API.Repositories;
using Northwind.API.Services;
using Northwind.Data.Entities;
using Xunit;

namespace Northwind.Tests.UnitTests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IRepository<User>> _userRepository;
        private readonly Mock<IOptions<AuthSettings>> _authSettings;
        private readonly IAuthService _authService;

        public AuthServiceTests()
        {
            _userRepository = new Mock<IRepository<User>>();
            _authSettings = new Mock<IOptions<AuthSettings>>();
            var cryptoService = new Mock<CryptoService> { CallBase = true };
            var logger = new Mock<ILogger<AuthService>>();

            MockAuthSettings();

            _authService = new AuthService(_userRepository.Object,
                                           cryptoService.Object,
                                           _authSettings.Object,
                                           logger.Object);
        }

        [Fact]
        public async Task UsernameWithWrongPassword_GetByCredentials_UserNotReturned()
        {
            var salt = new byte[64];
            var hash = new byte[32];
            var userInDb = new[] { new User { PasswordSalt = salt, PasswordHash = hash }  };

            _userRepository.Setup(ur => ur.FindByCondition(It.IsAny<Expression<Func<User, bool>>>()))
                           .Returns(userInDb.AsQueryable().BuildMock().Object);

            var user = await _authService.GetByCredentials("test username", "test password");

            Assert.Null(user);
        }

        [Fact]
        public void UserWithoutTokens_CreateCredentials_TokensGenerated()
        {
            var user = new User { UserName = "TestUserName" };

            _authSettings.Object.Value.ExpiryDuration = new TimeSpan(1, 0, 0, 0).Ticks;

            var userWithCredentials = _authService.CreateCredentials(user);

            Assert.NotNull(userWithCredentials.AccessToken);
            Assert.NotNull(userWithCredentials.RefreshToken.Value);
            Assert.NotNull(userWithCredentials.RefreshToken.CreateDate);
            Assert.NotNull(userWithCredentials.RefreshToken.ExpiryDate);
            Assert.Null(userWithCredentials.RefreshToken.RevokeDate);


            _userRepository.Verify(ur => ur.Update(It.IsAny<User>()));
        }

        [Fact]
        public void UserWithTokens_RefreshCredentials_NewTokensGenerated()
        {
            var user = new User
            {
                UserName = "TestUserName",
                AccessToken = "test access token",
                RefreshToken = new RefreshToken
                {
                    Value = "test refresh token",
                    CreateDate = DateTimeOffset.UtcNow.Subtract(new TimeSpan(2, 0, 0, 0)),
                    ExpiryDate = DateTimeOffset.UtcNow.Subtract(new TimeSpan(1, 0, 0, 0))
                }
            };

            _authSettings.Object.Value.ExpiryDuration = new TimeSpan(1, 0, 0, 0).Ticks;

            var userWithNewCred = _authService.RefreshCredentials(user);

            Assert.NotEqual("test access token", userWithNewCred.AccessToken);
            Assert.NotEqual("test refresh token", userWithNewCred.RefreshToken.Value);
            Assert.Equal(DateTime.UtcNow.Date, user.RefreshToken.CreateDate?.Date);
            Assert.Equal(DateTime.UtcNow.AddDays(2).Date, userWithNewCred.RefreshToken.ExpiryDate?.Date);
            Assert.Null(userWithNewCred.RefreshToken.RevokeDate);

            _userRepository.Verify(ur => ur.Update(It.IsAny<User>()));
        }

        [Fact]
        public void UserWithTokens_RevokeCredentials_TokensRemoved()
        {
            var currentDate = DateTimeOffset.UtcNow;

            var user = new User
            {
                UserName = "TestUserName",
                AccessToken = "test access token",
                RefreshToken = new RefreshToken
                {
                    Value = "test refresh token",
                    CreateDate = currentDate,
                    ExpiryDate = currentDate.Add(new TimeSpan(1, 0, 0))
                }
            };

            _authService.RevokeCredentials(user);

            Assert.Null(user.AccessToken);
            Assert.Null(user.RefreshToken.Value);
            Assert.Null(user.RefreshToken.ExpiryDate);

            Assert.Equal(currentDate, user.RefreshToken.CreateDate);
            Assert.Equal(DateTime.UtcNow.Date, user.RefreshToken.RevokeDate?.Date);

            _userRepository.Verify(ur => ur.Update(It.IsAny<User>()));
        }

        private void MockAuthSettings()
        {
            var mockAuthSetting = new AuthSettings
            {
                SecretKey = "3f117ac025d544f8",
                ExpiryDuration = new TimeSpan(0, 1, 0).Ticks
            };

            _authSettings.Setup(setting => setting.Value).Returns(mockAuthSetting);
        }
    }
}