using System;
using System.Text;
using AutoMapper;
using Northwind.API.Models;
using Northwind.API.Models.Auth;
using Northwind.API.Models.Users;
using Northwind.API.Profiles;
using Northwind.Data.Entities;
using Xunit;

namespace Northwind.Tests.UnitTests.Profiles
{
    public class UserProfileTests
    {
        private readonly IMapper _mapper;
        private static readonly Guid TestGuid = new Guid("a1130390-2ef2-486e-b93e-73e94c05a66e");

        public UserProfileTests()
        {
            var mapperConfig = new MapperConfiguration(config => config.AddProfile(new UserProfile()));
            _mapper = mapperConfig.CreateMapper();
        }

        [Fact]
        public void User_UserToAuthResponseModel_ReturnAuthResponseModel()
        {
            var user = CreateUser();

            var authResponse = _mapper.Map<AuthResponseModel>(user);

            Assert.Equal("TestAccessToken", authResponse.AccessToken);
            Assert.Equal("TestRefreshToken", authResponse.RefreshToken);
            Assert.Equal(DateTimeOffset.Parse("2020-09-10T12:00:00+00:00"), authResponse.RefreshTokenExpiryDate);
        }

        [Fact]
        public void AuthRequestModel_AuthRequestModelToUser_ReturnUser()
        {
            var authRequest = new AuthRequestModel
            {
                UserName = "TestName",
                Password = "pass1word",
            };

            var user = _mapper.Map<User>(authRequest);

            Assert.Equal("TestName", user.UserName);
            Assert.Equal(0, user.UserId);
            Assert.Null(user.RefreshToken);
            Assert.Null(user.PasswordHash);
            Assert.Null(user.PasswordSalt);
        }

        [Fact]
        public void User_UserToUserModel_ReturnUserModel()
        {
            var user = CreateUser();

            var userModel = _mapper.Map<UserModel>(user);

            Assert.Equal(3, userModel.UserId);
            Assert.Equal(TestGuid, userModel.UserIdentifier);
            Assert.Equal("TestName", userModel.UserName);
            Assert.Equal("TestAccessToken", userModel.AccessToken);

            var refreshTokenModel = userModel.RefreshToken;
            Assert.Equal("TestRefreshToken", refreshTokenModel.Value);
            Assert.Equal(DateTimeOffset.Parse("2020-09-10T12:00:00+00:00"), refreshTokenModel.ExpiryDate);
            Assert.Equal(DateTimeOffset.Parse("2020-09-09T15:30:00+00:00"), refreshTokenModel.CreateDate);
            Assert.Null(refreshTokenModel.RevokeDate);
            Assert.False(refreshTokenModel.IsRevoked);
        }

        [Fact]
        public void UserRequestModel_UserRequestModelToUser_ReturnUser()
        {
            var userRequestModel = new UserRequestModel
            {
                UserName = "testUser",
                Password = "pass1word",
                Role = "Shipper"
            };

            var user = _mapper.Map<User>(userRequestModel);

            Assert.Equal("testUser", user.UserName);
            Assert.Equal(Role.Shipper, user.Role);
            Assert.Equal(0, user.UserId);
            Assert.Null(user.RefreshToken);
            Assert.Null(user.PasswordHash);
            Assert.Null(user.PasswordSalt);
        }

        private static User CreateUser()
        {
            return new User
            {
                UserId = 3,
                UserIdentifier = TestGuid,
                UserName = "TestName",
                PasswordHash = Encoding.UTF8.GetBytes("testHash"),
                PasswordSalt = Encoding.UTF8.GetBytes("testSalt"),
                AccessToken = "TestAccessToken",
                RefreshToken = new RefreshToken
                {
                    RefreshTokenId = 3,
                    Value = "TestRefreshToken",
                    ExpiryDate = new DateTimeOffset(2020, 9, 10, 12, 00, 00, TimeSpan.Zero),
                    CreateDate = new DateTimeOffset(2020, 9, 9, 15, 30, 00, TimeSpan.Zero)
                }
            };
        }
    }
}