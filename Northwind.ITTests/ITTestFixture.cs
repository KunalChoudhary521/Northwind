using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Northwind.API;
using Northwind.Data.Contexts;
using Northwind.ITTests.Helpers;

namespace Northwind.ITTests
{
    public class ITTestFixture : IDisposable
    {
        private readonly ApiWebApplicationFactory<Startup> _factory;
        private readonly IServiceScope _serviceScope;

        public readonly HttpClient Client;
        public readonly NorthwindContext DbContext;
        public readonly UserHelper UserHelper;

        public ITTestFixture()
        {
            var integrationConfig = TestConstants.GetTestConfiguration(TestConstants.TestConfig);
            _factory = new ApiWebApplicationFactory<Startup>(integrationConfig, TestConstants.TestDbConnectionKey);
            Client = _factory.CreateClient();

            _serviceScope = _factory.Services.CreateScope();
            DbContext = _serviceScope.ServiceProvider.GetRequiredService<NorthwindContext>();
            var apiCredentials = integrationConfig.GetSection("ApiCredentials").Get<ApiCredentials>();

            SetAuthenticationHeader(apiCredentials.AccessToken);
            UserHelper = new UserHelper(Client);
        }

        public AuthenticationHeaderValue GetAuthenticationHeader() => Client.DefaultRequestHeaders.Authorization;

        public void SetAuthenticationHeader(string token)
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestConstants.AuthScheme, token);
        }


        public void Dispose()
        {
            _serviceScope.Dispose();
            _factory.Dispose();
        }
    }
}