using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Northwind.Data.Contexts;

namespace Northwind.ITTests
{
    public class ApiWebApplicationFactory<TStartup> :
        WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly IConfigurationRoot _config;
        private readonly string _testDbConnectionKey;

        public ApiWebApplicationFactory(IConfigurationRoot config, string testDbConnectionKey)
        {
            _config = config;
            _testDbConnectionKey = testDbConnectionKey;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(config => config.AddConfiguration(_config));

            // This callback runs after the API's Startup.ConfigureServices
            builder.ConfigureTestServices(services =>
            {
                // Remove DBContext used by API
                var descriptor = services.SingleOrDefault(d => d.ServiceType ==
                         typeof(DbContextOptions<NorthwindContext>));
                services.Remove(descriptor);

                // Add DBContext to be only used by ITTests
                services.AddDbContext<NorthwindContext>(options =>
                {
                    var connString = _config.GetConnectionString(_testDbConnectionKey);
                    options.UseNpgsql(connString)
                           .UseLoggerFactory(LoggerFactory.Create(logBuilder => logBuilder.AddConsole()));
                });
            });
        }
    }
}