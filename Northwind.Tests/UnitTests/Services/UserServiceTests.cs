using Microsoft.Extensions.Logging;
using Moq;
using Northwind.API.Repositories;
using Northwind.API.Services;
using Northwind.Data.Entities;
using Xunit;

namespace Northwind.Tests.UnitTests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IRepository<User>> _userRepository;
        private readonly IUserService _userService;

        public UserServiceTests()
        {
            _userRepository = new Mock<IRepository<User>>();
            var cryptoService = new Mock<CryptoService> { CallBase = true };
            var logger = new Mock<ILogger<UserService>>();

            _userService = new UserService(_userRepository.Object, cryptoService.Object, logger.Object);
        }

        [Fact]
        public void UserAndPasswordInPlainText_Add_PasswordEncryptedAndUserPersistInDb()
        {
            var user = new User { UserName = "testUser" };
            const string password = "testPass";

            _userService.Add(user, password);

            Assert.Equal(64, user.PasswordSalt.Length);
            Assert.Equal(32, user.PasswordHash.Length);
            _userRepository.Verify(ur => ur.Add(It.IsAny<User>()));
        }
    }
}