using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Northwind.API.Models.Users;
using Xunit;

namespace Northwind.ITTests.Helpers
{
    public class UserHelper
    {
        private const string BaseAuthPath = "/api/auth";
        private const string BaseUserPath = "/api/users";

        private readonly HttpClient _client;

        public UserHelper(HttpClient client)
        {
            _client = client;
        }

        public async Task<HttpResponseMessage> CreateUser(UserRequestModel userRequestModel)
        {
            var postResponse = await _client.PostAsJsonAsync(BaseUserPath, userRequestModel);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
            Assert.NotNull(postResponse.Headers.Location);

            return postResponse;
        }

        public async Task<HttpResponseMessage> CreateCredentials(UserRequestModel userRequestModel)
        {
            var postResponse = await _client.PostAsJsonAsync($"{BaseAuthPath}/access", userRequestModel);
            Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);

            return postResponse;
        }

        public async Task DeleteUser(int userId)
        {
            var deleteResponse = await _client.DeleteAsync($"{BaseUserPath}/{userId}");
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }
    }
}