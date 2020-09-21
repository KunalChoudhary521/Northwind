using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Northwind.API.Controllers;
using Northwind.API.Models;
using Northwind.API.Models.Users;
using Northwind.API.Profiles;
using Northwind.API.Services;
using Northwind.Data.Entities;
using Xunit;

namespace Northwind.Tests.UnitTests.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _userService;
        private readonly UsersController _usersController;

        public UserControllerTests()
        {
            var mapperConfig = new MapperConfiguration(config => config.AddProfile(new UserProfile()));
            var mapper = mapperConfig.CreateMapper();
            _userService = new Mock<IUserService>();

            _usersController = new UsersController(_userService.Object, mapper);
        }

        [Fact]
        public async Task IdOfNonExistentUser_GetUser_ReturnNotFound()
        {
            const int userId = -1;

            var response = await _usersController.GetUser(userId);

            Assert.IsType<ActionResult<UserModel>>(response);
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task UserNameOfNonExistentUser_GetUser_ReturnNotFound()
        {
            const string userName = "Unknown";

            var response = await _usersController.GetUser(userName);

            Assert.IsType<ActionResult<UserModel>>(response);
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task NonAdminUserCheckingStrangerUserInfo_GetUser_ReturnForbidden()
        {
            const string userName = "Unknown";

            var mockUser = new User { UserIdentifier = new Guid("521dc2cd-9995-45c1-878f-edc870830204") };
            _userService.Setup(us => us.GetByUserName(It.IsAny<string>())).ReturnsAsync(mockUser);

            var claimIdentities = new List<ClaimsIdentity>
            {
                new ClaimsIdentity(new []
                {
                    new Claim(ClaimTypes.NameIdentifier, "96da9a93-5dff-4622-92c9-1aeba1c311e4"),
                    new Claim(ClaimTypes.Role, "NotAdmin")
                })
            };
            _usersController.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(claimIdentities)
            };

            var response = await _usersController.GetUser(userName);

            Assert.IsType<ActionResult<UserModel>>(response);
            Assert.IsType<ForbidResult>(response.Result);
        }

        [Fact]
        public async Task UserNameAlreadyExists_AddUser_ReturnBadRequest()
        {
            var userRequestModel = new UserRequestModel { UserName = "User1", Password = "1234" };

            var mockUser = new User { UserName = "User1" };
            _userService.Setup(us => us.GetByUserName(It.IsAny<string>())).ReturnsAsync(mockUser);

            var response = await _usersController.AddUser(userRequestModel);

            Assert.IsType<ActionResult<UserModel>>(response);
            var result = Assert.IsType<BadRequestObjectResult>(response.Result);
            var message = result.Value as string ?? string.Empty;
            Assert.Contains("already exists", message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task NonExistentRole_AddUser_ReturnBadRequest()
        {
            var userRequestModel = new UserRequestModel { UserName = "User1", Password = "1234", Role = "da9a93" };

            var response = await _usersController.AddUser(userRequestModel);

            Assert.IsType<ActionResult<UserModel>>(response);
            var result = Assert.IsType<BadRequestObjectResult>(response.Result);
            var message = result.Value as string ?? string.Empty;
            Assert.Contains("role", message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("invalid", message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task FailedToSaveNewUser_AddUser_ReturnBadRequest()
        {
            var userRequestModel = new UserRequestModel { UserName = "User1", Password = "1234", Role = "Admin" };

            var response = await _usersController.AddUser(userRequestModel);

            Assert.IsType<ActionResult<UserModel>>(response);
            Assert.IsType<BadRequestResult>(response.Result);
        }

        [Fact]
        public async Task UserRequest_AddUser_ReturnResponseWithLocationInHeader()
        {
            var userRequestModel = new UserRequestModel { UserName = "User1", Password = "1234", Role = "Admin" };

            _userService.Setup(us => us.IsSavedToDb()).ReturnsAsync(true);

            var response = await _usersController.AddUser(userRequestModel);

            Assert.IsType<ActionResult<UserModel>>(response);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(response.Result);

            Assert.IsType<UserModel>(createdAtActionResult.Value);
            Assert.Equal(StatusCodes.Status201Created, createdAtActionResult.StatusCode);
            Assert.Single(createdAtActionResult.RouteValues.Keys, "userId");
        }

        [Fact]
        public async Task IdOfNonExistentUser_DeleteUser_ReturnNotFound()
        {
            const int userId = -1;

            var response = await _usersController.DeleteUser(userId);

            Assert.IsType<NotFoundResult>(response);
        }

        [Fact]
        public async Task FailedToSaveDeletedUser_DeleteUser_ReturnBadRequest()
        {
            const int userId = 3;

            _userService.Setup(us => us.GetById(It.IsAny<int>())).ReturnsAsync(new User());

            var response = await _usersController.DeleteUser(userId);

            Assert.IsType<BadRequestResult>(response);
        }

    }
}