using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Northwind.API.Models.Auth;
using Northwind.API.Models.Users;
using Xunit;

namespace Northwind.ITTests.E2ETests
{
    [Collection(TestConstants.ItTests)]
    public class AuthControllerE2E
    {
        private const string BaseAuthPath = "/api/auth";
        private const string BaseUserPath = "/api/users";
        private readonly HttpClient _client;
        private readonly Action<string> _setAuthHeader;
        private readonly Func<AuthenticationHeaderValue> _getAuthHeader;

        public AuthControllerE2E(ITTestFixture fixture)
        {
            _client = fixture.Client;
            _getAuthHeader = fixture.GetAuthenticationHeader;
            _setAuthHeader = fixture.SetAuthenticationHeader;
        }

        [Fact]
        public async Task ApiUserCredentials_CallAuthEndpoints_CreateRefreshRevokeCredentials()
        {
            var userRequest = new UserRequestModel
            {
                UserName = "authTestUser",
                Password = "pass1word",
                Role = "Supplier"
            };

            // CREATE User
            var createUserResponse = await _client.PostAsJsonAsync(BaseUserPath, userRequest);
            Assert.Equal(HttpStatusCode.Created, createUserResponse.StatusCode);
            Assert.NotNull(createUserResponse.Headers.Location);

            // CREATE Credentials
            var createCredResponse = await _client.PostAsJsonAsync($"{BaseAuthPath}/access", userRequest);
            Assert.Equal(HttpStatusCode.OK, createCredResponse.StatusCode);

            var userWithTokens = await createCredResponse.Content.ReadAsAsync<AuthResponseModel>();
            Assert.NotNull(userWithTokens.AccessToken);
            Assert.NotNull(userWithTokens.RefreshToken);
            Assert.NotEqual(DateTimeOffset.MinValue.Date, userWithTokens.RefreshTokenExpiryDate.Date);

            // Allow time to pass so new token is generated
            Thread.Sleep(new TimeSpan(0, 0, 1));

            // REFRESH
            var refreshTokenRequest = new RefreshTokenRequest { RefreshToken = userWithTokens.RefreshToken };

            var refreshResponse = await _client.PostAsJsonAsync($"{BaseAuthPath}/refresh", refreshTokenRequest);
            Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

            var userWithNewTokens = await refreshResponse.Content.ReadAsAsync<AuthResponseModel>();
            Assert.NotEqual(userWithTokens.AccessToken, userWithNewTokens.AccessToken);
            Assert.NotEqual(userWithTokens.RefreshToken, userWithNewTokens.RefreshToken);

            // REVOKE
            refreshTokenRequest = new RefreshTokenRequest { RefreshToken = userWithNewTokens.RefreshToken };
            var revokeResponse = await _client.PostAsJsonAsync($"{BaseAuthPath}/revoke", refreshTokenRequest);
            Assert.Equal(HttpStatusCode.OK, revokeResponse.StatusCode);

            // Try to refresh token
            refreshResponse = await _client.PostAsJsonAsync($"{BaseAuthPath}/refresh", refreshTokenRequest);
            Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);

            // DELETE User
            var userId = int.Parse(createUserResponse.Headers.Location.Segments.Last());
            var deleteResponse = await _client.DeleteAsync($"{BaseUserPath}/{userId}");
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task NonAdminUser_RevokeCredentials_ReturnForbidden()
        {
            var userRequest = new UserRequestModel
            {
                UserName = "nonAdmin",
                Password = "pass1word",
                Role = "SupplierAdmin"
            };

            // CREATE User & Credentials
            var createUserResponse = await _client.PostAsJsonAsync(BaseUserPath, userRequest);
            Assert.Equal(HttpStatusCode.Created, createUserResponse.StatusCode);

            var createCredResponse = await _client.PostAsJsonAsync($"{BaseAuthPath}/access", userRequest);
            Assert.Equal(HttpStatusCode.OK, createCredResponse.StatusCode);
            var nonAdminUser = await createCredResponse.Content.ReadAsAsync<AuthResponseModel>();

            var initialToken = _getAuthHeader().Parameter;

            // Try REVOKE
            _setAuthHeader(nonAdminUser.AccessToken);
            var revokeResponse = await _client.PostAsJsonAsync($"{BaseAuthPath}/revoke", nonAdminUser.RefreshToken);
            Assert.Equal(HttpStatusCode.Forbidden, revokeResponse.StatusCode);

            // DELETE User
            _setAuthHeader(initialToken);
            var userId = int.Parse(createUserResponse.Headers.Location.Segments.Last());
            var deleteResponse = await _client.DeleteAsync($"{BaseUserPath}/{userId}");
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }
    }
}