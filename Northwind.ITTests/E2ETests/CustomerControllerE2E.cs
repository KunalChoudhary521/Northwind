using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Northwind.API.Models;
using Northwind.API.Models.Orders;
using Xunit;

namespace Northwind.ITTests.E2ETests
{
    [Collection(TestConstants.ItTests)]
    public class CustomerControllerE2E
    {
        private const string BaseCustomerPath = "/api/customers";
        private readonly HttpClient _client;

        public CustomerControllerE2E(ITTestFixture fixture)
        {
            _client = fixture.Client;
        }

        [Fact]
        public async Task CustomersFromSeedData_GetCustomers_CustomersReturned()
        {
            var getResponse = await _client.GetAsync(BaseCustomerPath);

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var customers = await getResponse.Content.ReadAsAsync<CustomerModel[]>();
            Assert.Equal(2, customers.Length);
        }

        [Fact]
        public async Task CustomerModel_CallCustomerEndpoints_CreateUpdateDeleteCustomer()
        {
            var customerModel = new CustomerModel
            {
                CompanyName = "E2E customer CRUD",
                ContactName = "Mr. Cust",
                Location = new LocationModel
                {
                    Address = "10 Spectrum Way",
                    City = "Mississauga"
                }
            };

            // CREATE
            var postResponse = await _client.PostAsJsonAsync(BaseCustomerPath, customerModel);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
            Assert.NotNull(postResponse.Headers.Location);

            int customerId = int.Parse(postResponse.Headers.Location.Segments.Last());

            // Retrieve newly created customer
            var getResponse = await _client.GetAsync($"{BaseCustomerPath}/{customerId}");

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var customerRetrieved = await getResponse.Content.ReadAsAsync<CustomerModel>();
            Assert.Equal("E2E customer CRUD", customerRetrieved.CompanyName);
            Assert.Equal("Mr. Cust", customerRetrieved.ContactName);
            Assert.Null(customerRetrieved.ContactTitle);
            Assert.Equal("10 Spectrum Way", customerRetrieved.Location.Address);
            Assert.Equal("Mississauga", customerRetrieved.Location.City);
            Assert.Null(customerRetrieved.Location.Region);

            // UPDATE
            customerModel.CompanyName = "Update operation - E2E customer CRUD";
            customerModel.Location.Region = "Ontario";

            var putResponse = await _client.PutAsJsonAsync($"{BaseCustomerPath}/{customerId}", customerModel);

            Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

            var customerUpdated = await putResponse.Content.ReadAsAsync<CustomerModel>();
            Assert.Equal("Update operation - E2E customer CRUD", customerUpdated.CompanyName);
            Assert.Equal("Ontario", customerUpdated.Location.Region);

            // DELETE
            var deleteResponse = await _client.DeleteAsync($"{BaseCustomerPath}/{customerId}");
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

            // Try to retrieve deleted customer
            getResponse = await _client.GetAsync($"{BaseCustomerPath}/{customerId}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task CustomerOrdersFromSeedData_GetCustomerOrders_OrdersReturned()
        {
            const int customerId = 1;
            var getResponse = await _client.GetAsync($"{BaseCustomerPath}/{customerId}/orders");

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var orders = await getResponse.Content.ReadAsAsync<OrderResponseModel[]>();
            Assert.Equal(2, orders.Length);
        }

        [Fact]
        public async Task OrderRequestModel_CallCustomerOrderEndpoints_CreateUpdateDeleteOrder()
        {
            const int customerId = 2;
            var customerOrderUri = $"{BaseCustomerPath}/{customerId}/orders";

            var orderRequestModel = new OrderRequestModel
            {
                RequiredDate = DateTimeOffset.Parse("2020-08-31T20:00:00+00:00"),
                OrderItems = new []
                {
                    new OrderItemRequestModel { ProductId = 1, Quantity = 5},
                    new OrderItemRequestModel { ProductId = 2, Quantity = 2},
                    new OrderItemRequestModel { ProductId = 3, Quantity = 3}
                }
            };

            // CREATE
            var postResponse = await _client.PostAsJsonAsync(customerOrderUri, orderRequestModel);

            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
            Assert.NotNull(postResponse.Headers.Location);

            int orderId = int.Parse(postResponse.Headers.Location.Segments.Last());

            // Retrieve newly created product
            var getResponse = await _client.GetAsync($"{customerOrderUri}/{orderId}");

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var orderRetrieved = await getResponse.Content.ReadAsAsync<OrderResponseModel>();
            Assert.Equal(customerId, orderRetrieved.CustomerId);
            Assert.Equal(new DateTimeOffset(2020, 8, 31, 20, 0, 0, TimeSpan.Zero),
                         orderRetrieved.RequiredDate);
            Assert.Equal(new decimal(23.50), orderRetrieved.Total);
            Assert.Equal(DateTime.Today, orderRetrieved.OrderDate.Date);
            Assert.Null(orderRetrieved.ShippedDate);

            Assert.Equal(3, orderRetrieved.OrderItems.Count);
            var orderItems = orderRetrieved.OrderItems.ToArray();
            Assert.Equal(2, orderItems[1].ProductId);
            Assert.Contains("product 2", orderItems[1].ProductName, StringComparison.Ordinal);
            Assert.Equal(new decimal(2.79), orderItems[1].UnitPrice);

            // UPDATE
            var orderWithRequiredDate = new OrderModelBase
            {
                RequiredDate = DateTimeOffset.Parse("2020-09-05T14:30:00+00:00")
            };

            var putResponse = await _client.PutAsJsonAsync($"{customerOrderUri}/{orderId}",
                                                           orderWithRequiredDate);

            Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
            var orderUpdated = await putResponse.Content.ReadAsAsync<OrderResponseModel>();
            Assert.Equal(new DateTimeOffset(2020, 9, 5, 14, 30, 0, TimeSpan.Zero),
                         orderUpdated.RequiredDate);
            Assert.Equal(3, orderUpdated.OrderItems.Count);

            // DELETE
            var deleteResponse = await _client.DeleteAsync($"{customerOrderUri}/{orderId}");
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

            // Try to retrieve deleted order
            getResponse = await _client.GetAsync($"{customerOrderUri}/{orderId}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }
    }
}