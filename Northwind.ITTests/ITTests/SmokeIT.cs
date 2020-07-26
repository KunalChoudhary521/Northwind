using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Northwind.Data.Contexts;
using Northwind.Data.Entities;
using Xunit;

namespace Northwind.ITTests.ITTests
{
    [Collection(TestConstants.ItTests)]
    public class SmokeIT
    {
        private readonly HttpClient _client;
        private readonly NorthwindContext _dbContext;

        public SmokeIT(ITTestFixture fixture)
        {
            _client = fixture.Client;
            _dbContext = fixture.DbContext;
        }

        [Fact]
        public async Task RunningAPI_CallHealthEndpoint_ReturnOkResponse()
        {
            var healthResponse = await _client.GetAsync("/api/health");
            Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);
        }

        [Fact]
        public async Task RunningDatabase_CanConnect_ReturnTrue()
        {
            Assert.True(await _dbContext.Database.CanConnectAsync(),
                        "Error: Unable to connect to database");
        }

        [Fact]
        public async Task swaggerUi_CallSwaggerEndpoint_ReturnOkResponse()
        {
            var swaggerResponse = await _client.GetAsync("/swagger");
            Assert.Equal(HttpStatusCode.OK, swaggerResponse.StatusCode);
        }

        [Fact]
        public async Task RunningDatabase_ContainsSeedData_ReturnInitialDbState()
        {
            var suppliers = await _dbContext.Set<Supplier>().ToArrayAsync();
            var locations = await _dbContext.Set<Location>().ToArrayAsync();
            var categories = await _dbContext.Set<Category>().ToArrayAsync();
            var products = await _dbContext.Set<Product>().ToArrayAsync();
            var customers = await _dbContext.Set<Customer>().ToArrayAsync();
            var orders = await _dbContext.Set<Order>().ToArrayAsync();
            var orderDetails = await _dbContext.Set<OrderDetail>().ToArrayAsync();
            var shippers = await _dbContext.Set<Shipper>().ToArrayAsync();
            var users = await _dbContext.Set<User>().ToArrayAsync();

            Assert.Equal(2, suppliers.Length);
            var supplierIds = suppliers.Select(s => s.SupplierId).ToArray();
            Assert.Contains(1, supplierIds);
            Assert.Contains(2, supplierIds);

            Assert.Equal(2, locations.Length);
            Assert.Equal(2, categories.Length);
            Assert.Equal(4, products.Length);

            Assert.Equal(2, customers.Length);
            Assert.Equal(3, orders.Length);
            Assert.Equal(4, orderDetails.Length);

            Assert.Equal(2, shippers.Length);
            Assert.Single(users);
        }
    }
}