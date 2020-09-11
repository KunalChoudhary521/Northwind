using System;
using System.Threading.Tasks;
using AutoMapper;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Northwind.API.Controllers;
using Northwind.API.Models.Auth;
using Northwind.API.Profiles;
using Northwind.API.Services;
using Northwind.Data.Entities;
using Xunit;

namespace Northwind.Tests.UnitTests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authService;
        private readonly AuthController _authController;

        public AuthControllerTests()
        {
            var mapperConfig = new MapperConfiguration(config => config.AddProfile(new UserProfile()));
            var mapper = mapperConfig.CreateMapper();
            _authService = new Mock<IAuthService>();

            _authController = new AuthController(_authService.Object, mapper);
        }

        [Fact]
        public async Task UserInfo_CreateCredentials_ReturnUnauthorized()
        {
            var authRequestModel = new AuthRequestModel { UserName = "testUser", Password = "testPass" };

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _authController.CreateCredentials(authRequestModel));

            Assert.Equal(StatusCodes.Status401Unauthorized, exception.Details.Status);
            Assert.Contains("invalid", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("credentials", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task FailedToCreate_CreateCredentials_ReturnBadRequest()
        {
            var authRequestModel = new AuthRequestModel { UserName = "testUser", Password = "testPass" };

            _authService.Setup(service => service.GetByCredentials(It.IsAny<string>(), It.IsAny<string>()))
                        .ReturnsAsync(new User());

            var response = await _authController.CreateCredentials(authRequestModel);

            Assert.IsType<ActionResult<AuthResponseModel>>(response);
            Assert.IsType<BadRequestResult>(response.Result);
            _authService.Verify(s => s.IsSavedToDb());
        }

        [Fact]
        public async Task IncorrectToken_RefreshCredentials_ReturnUnauthorized()
        {
            var refreshTokenRequest = new RefreshTokenRequest { RefreshToken = "testToken" };

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _authController.RefreshCredentials(refreshTokenRequest));

            Assert.Equal(StatusCodes.Status401Unauthorized, exception.Details.Status);
            Assert.Contains("invalid", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("refresh", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ExpiredRefreshToken_RefreshCredentials_ReturnUnauthorized()
        {
            var refreshTokenRequest = new RefreshTokenRequest { RefreshToken = "testToken" };

            var mockUser = new User { RefreshToken = new RefreshToken { ExpiryDate = DateTimeOffset.MinValue } };
            _authService.Setup(service => service.GetByRefreshToken(It.IsAny<string>())).ReturnsAsync(mockUser);

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _authController.RefreshCredentials(refreshTokenRequest));

            Assert.Equal(StatusCodes.Status401Unauthorized, exception.Details.Status);
            Assert.Contains("expire", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("refresh", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task FailedToRefresh_RefreshCredentials_ReturnBadRequest()
        {
            var refreshTokenRequest = new RefreshTokenRequest { RefreshToken = "testToken" };

            var mockUser = new User { RefreshToken = new RefreshToken { ExpiryDate = DateTimeOffset.MaxValue } };
            _authService.Setup(service => service.GetByRefreshToken(It.IsAny<string>())).ReturnsAsync(mockUser);

            var response = await _authController.RefreshCredentials(refreshTokenRequest);

            Assert.IsType<ActionResult<AuthResponseModel>>(response);
            Assert.IsType<BadRequestResult>(response.Result);
            _authService.Verify(s => s.IsSavedToDb());
        }

        [Fact]
        public async Task IncorrectToken_RevokeCredentials_ReturnUnauthorized()
        {
            var refreshTokenRequest = new RefreshTokenRequest { RefreshToken = "testToken" };

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _authController.RevokeCredentials(refreshTokenRequest));

            Assert.Equal(StatusCodes.Status401Unauthorized, exception.Details.Status);
            Assert.Contains("invalid", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("refresh", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task FailedToRevoke_RevokeCredentials_ReturnBadRequest()
        {
            var refreshTokenRequest = new RefreshTokenRequest { RefreshToken = "testToken" };

            _authService.Setup(service => service.GetByRefreshToken(It.IsAny<string>())).ReturnsAsync(new User());

            var response = await _authController.RevokeCredentials(refreshTokenRequest);

            Assert.IsType<BadRequestResult>(response);
            _authService.Verify(s => s.IsSavedToDb());
        }
    }
}