using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Northwind.API.Models;
using Northwind.API.Models.Auth;
using Northwind.API.Models.Users;
using Northwind.ITTests.Helpers;
using Xunit;

namespace Northwind.ITTests.E2ETests
{
    [Collection(TestConstants.ItTests)]
    public class UserControllerE2E
    {
        private const string BaseUserPath = "/api/users";
        private const string BaseAuthPath = "/api/auth";
        private readonly HttpClient _client;
        private readonly UserHelper _userHelper;
        private readonly Action<string> _setAuthHeader;
        private readonly Func<AuthenticationHeaderValue> _getAuthHeader;

        public UserControllerE2E(ITTestFixture fixture)
        {
            _client = fixture.Client;
            _userHelper = fixture.UserHelper;
            _getAuthHeader = fixture.GetAuthenticationHeader;
            _setAuthHeader = fixture.SetAuthenticationHeader;
        }

        [Fact]
        public async Task AdminUserCredentials_CallUserEndpoints_CreateRetrieveDeleteUser()
        {
            var userRequestModel = new UserRequestModel
            {
                UserName = "testUser",
                Password = "pass1word",
                Role = "Admin"
            };

            // CREATE
            var postResponse = await _userHelper.CreateUser(userRequestModel);
            var userId = int.Parse(postResponse.Headers.Location.Segments.Last());
            var newlyCreatedUser = await postResponse.Content.ReadAsAsync<UserModel>();

            Assert.Equal(userRequestModel.UserName, newlyCreatedUser.UserName);
            Assert.Null(newlyCreatedUser.AccessToken);
            Assert.Null(newlyCreatedUser.RefreshToken);

            // RETRIEVE
            var getByIdResponse = await _client.GetAsync($"{BaseUserPath}/{userId}");
            Assert.Equal(HttpStatusCode.OK, getByIdResponse.StatusCode);

            var userRetrieved = await getByIdResponse.Content.ReadAsAsync<UserModel>();
            Assert.Equal(userRequestModel.UserName, userRetrieved.UserName);
            Assert.Null(userRetrieved.AccessToken);
            Assert.Null(userRetrieved.RefreshToken);

            var getByUsernameResponse = await _client.GetAsync($"{BaseUserPath}/{userRetrieved.UserName}");
            Assert.Equal(HttpStatusCode.OK, getByUsernameResponse.StatusCode);

            userRetrieved = await getByUsernameResponse.Content.ReadAsAsync<UserModel>();
            Assert.Equal(userRequestModel.UserName, userRetrieved.UserName);
            Assert.Null(userRetrieved.AccessToken);
            Assert.Null(userRetrieved.RefreshToken);

            // DELETE
            await _userHelper.DeleteUser(userRetrieved.UserId);

            // Try to retrieve deleted user
            var getResponse = await _client.GetAsync($"{BaseUserPath}/{userRetrieved.UserId}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task NonAdminUserCredentials_CallUserEndpoints_ReturnForbidden()
        {
            var userRequestModel = new UserRequestModel
            {
                UserName = "testUser",
                Password = "pass1word",
                Role = "Customer"
            };

            // Create test User & Credentials (with Admin token)
            var postResponse = await _userHelper.CreateUser(userRequestModel);
            var userId = int.Parse(postResponse.Headers.Location.Segments.Last());

            var createCredResponse = await _userHelper.CreateCredentials(userRequestModel);
            var nonAdminUser = await createCredResponse.Content.ReadAsAsync<AuthResponseModel>();

            var initialToken = _getAuthHeader().Parameter;
            _setAuthHeader(nonAdminUser.AccessToken);

            // CREATE
            var nonAdminResponse = await _client.PostAsJsonAsync(BaseUserPath, userRequestModel);
            Assert.Equal(HttpStatusCode.Forbidden, nonAdminResponse.StatusCode);

            // DELETE
            var deleteResponse = await _client.DeleteAsync($"{BaseUserPath}/{userId}");
            Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);

            // GET ALL
            var getAllResponse = await _client.GetAsync($"{BaseUserPath}");
            Assert.Equal(HttpStatusCode.Forbidden, getAllResponse.StatusCode);

            // GET BY ID
            var getByIdResponse = await _client.GetAsync($"{BaseUserPath}/{userId}");
            Assert.Equal(HttpStatusCode.Forbidden, getByIdResponse.StatusCode);

            // GET BY username
            var getByUsernameResponse = await _client.GetAsync($"{BaseUserPath}/{userRequestModel.UserName}");
            Assert.Equal(HttpStatusCode.OK, getByUsernameResponse.StatusCode);

            // DELETE (with Admin token)
            _setAuthHeader(initialToken);
            deleteResponse = await _client.DeleteAsync($"{BaseUserPath}/{userId}");
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task NonAdminUserRetrievingStrangerUserInfo_GetUser_ReturnForbidden()
        {
            // Create testUser1 & Credentials (with Admin token)
            var testUser1Request = new UserRequestModel
            {
                UserName = "testUser1",
                Password = "pass1word",
                Role = "Customer"
            };
            var postResponse = await _userHelper.CreateUser(testUser1Request);
            var user1Id = int.Parse(postResponse.Headers.Location.Segments.Last());

            var createCredResponse = await _userHelper.CreateCredentials(testUser1Request);
            var testUser1 = await createCredResponse.Content.ReadAsAsync<AuthResponseModel>();

            // Create testUser2 & Credentials (with Admin token)
            var testUser2Request = new UserRequestModel
            {
                UserName = "testUser2",
                Password = "pass2word",
                Role = "Customer"
            };
            postResponse = await _userHelper.CreateUser(testUser2Request);
            var user2Id = int.Parse(postResponse.Headers.Location.Segments.Last());

            createCredResponse = await _userHelper.CreateCredentials(testUser2Request);
            var testUser2 = await createCredResponse.Content.ReadAsAsync<AuthResponseModel>();

            var initialToken = _getAuthHeader().Parameter;

            // Try to retrieve testUser2 with testUser1's access token
            _setAuthHeader(testUser1.AccessToken);
            var getByUsernameResponse = await _client.GetAsync($"{BaseUserPath}/{testUser2Request.UserName}");
            Assert.Equal(HttpStatusCode.Forbidden, getByUsernameResponse.StatusCode);

            // Try to retrieve testUser1 with testUser2's access token
            _setAuthHeader(testUser2.AccessToken);
            getByUsernameResponse = await _client.GetAsync($"{BaseUserPath}/{testUser1Request.UserName}");
            Assert.Equal(HttpStatusCode.Forbidden, getByUsernameResponse.StatusCode);

            // Delete test users (using Admin token)
            _setAuthHeader(initialToken);
            var deleteResponse = await _client.DeleteAsync($"{BaseUserPath}/{user1Id}");
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

            deleteResponse = await _client.DeleteAsync($"{BaseUserPath}/{user2Id}");
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task MultipleUsers_GetAllUsers_UsersReturned()
        {
            var getAllResponse = await _client.GetAsync($"{BaseUserPath}");
            Assert.Equal(HttpStatusCode.OK, getAllResponse.StatusCode);

            var users = await getAllResponse.Content.ReadAsAsync<UserModel[]>();

            Assert.Single(users);
            Assert.False(string.IsNullOrEmpty(users[0].AccessToken));
            Assert.NotNull(users[0].RefreshToken);
        }
    }
}