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
    public class ShipperControllerE2E
    {
        private const string BaseShipperPath = "/api/shippers";
        private const string BaseCustomerPath = "/api/customers";
        private readonly HttpClient _client;

        public ShipperControllerE2E(ITTestFixture fixture)
        {
            _client = fixture.Client;
        }

        [Fact]
        public async Task ShippersFromSeedData_GetShippers_ShippersReturned()
        {
            var getResponse = await _client.GetAsync(BaseShipperPath);

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var shippers = await getResponse.Content.ReadAsAsync<ShipperModel[]>();
            Assert.Equal(2, shippers.Length);
        }

        [Fact]
        public async Task ShipperModel_CallShipperEndpoints_CreateUpdateDeleteShipper()
        {
            var shipperModel = new ShipperModel { CompanyName = "E2E shipper CRUD" };

            // CREATE
            var postResponse = await _client.PostAsJsonAsync(BaseShipperPath, shipperModel);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
            Assert.NotNull(postResponse.Headers.Location);

            int shipperId = int.Parse(postResponse.Headers.Location.Segments.Last());

            // Retrieve newly created shipper
            var getResponse = await _client.GetAsync($"{BaseShipperPath}/{shipperId}");

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var shipperRetrieved = await getResponse.Content.ReadAsAsync<ShipperModel>();
            Assert.Equal("E2E shipper CRUD", shipperRetrieved.CompanyName);
            Assert.Null(shipperRetrieved.Phone);

            // UPDATE
            shipperModel.CompanyName = "Update operation - E2E shipper";
            shipperModel.Phone = "(501) 777-2231";

            var putResponse = await _client.PutAsJsonAsync($"{BaseShipperPath}/{shipperId}",
                                                           shipperModel);

            Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

            var shipperUpdated = await putResponse.Content.ReadAsAsync<ShipperModel>();
            Assert.Equal("Update operation - E2E shipper", shipperUpdated.CompanyName);
            Assert.Equal("(501) 777-2231", shipperUpdated.Phone);

            // DELETE
            var deleteResponse = await _client.DeleteAsync($"{BaseShipperPath}/{shipperId}");
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

            // Try to retrieve deleted shipper
            getResponse = await _client.GetAsync($"{BaseShipperPath}/{shipperId}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task ShipperFromSeedData_GetShipperOrders_OrdersReturned()
        {
            const int shipperId = 1;
            var getResponse = await _client.GetAsync($"{BaseShipperPath}/{shipperId}/orders");

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var orders = await getResponse.Content.ReadAsAsync<OrderResponseModel[]>();
            Assert.Equal(2, orders.Length);
        }

        [Fact]
        public async Task OrderModel_CallShipperOrderEndpoints_UpdateDeleteOrder()
        {
            const int shipperId = 2;
            const int orderId = 3;
            var shipperOrderUrl = $"{BaseShipperPath}/{shipperId}/orders";

            // UPDATE
            var shipperOrderModel = new ShipperOrderModel
            {
                ShipName = "Atlantic",
                ShippedDate = DateTimeOffset.Parse("2020-08-23T10:45:00+10:00")
            };

            var putResponse = await _client.PutAsJsonAsync($"{shipperOrderUrl}/{orderId}",
                                                           shipperOrderModel);

            Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
            var orderUpdated = await putResponse.Content.ReadAsAsync<OrderResponseModel>();
            Assert.Equal(2, orderUpdated.ShipperId);
            Assert.Equal(new DateTimeOffset(2020, 8, 23, 10, 45, 0, new TimeSpan(10, 0, 0)),
                         orderUpdated.ShippedDate);
            Assert.Equal("Atlantic", orderUpdated.ShipName);

            // DELETE (detach order from shipper)
            var deleteResponse = await _client.DeleteAsync($"{shipperOrderUrl}/{orderId}");
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

            // Try to retrieve detached order
            var getResponse = await _client.GetAsync($"{shipperOrderUrl}/{orderId}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

            // Ensure order is detached from shipper
            var customerId = orderUpdated.CustomerId;
            getResponse = await _client.GetAsync($"{BaseCustomerPath}/{customerId}/orders/{orderId}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var detachedOrder = await getResponse.Content.ReadAsAsync<OrderResponseModel>();
            Assert.Null(detachedOrder.ShipperId);
            Assert.Null(detachedOrder.ShippedDate);
            Assert.Null(detachedOrder.ShipName);
        }

        [Fact]
        public async Task ShipperWithOrders_DeleteShipper_OrdersDetachedFromShipper()
        {
            const int customerId = 2;
            var customerOrderUri = $"{BaseCustomerPath}/{customerId}/orders";

            var orderRequestModel = new OrderRequestModel
            {
                RequiredDate = DateTimeOffset.Parse("2020-09-21T11:00:00+00:00"),
                OrderItems = new []
                {
                    new OrderItemRequestModel { ProductId = 2, Quantity = 2},
                    new OrderItemRequestModel { ProductId = 3, Quantity = 2},
                    new OrderItemRequestModel { ProductId = 4, Quantity = 1}
                }
            };

            // Add order to customer
            var postResponse = await _client.PostAsJsonAsync(customerOrderUri, orderRequestModel);

            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
            Assert.NotNull(postResponse.Headers.Location);

            var orderId = int.Parse(postResponse.Headers.Location.Segments.Last());

            // Add shipper
            var shipperModel = new ShipperModel { CompanyName = "E2E shipper with orders" };

            postResponse = await _client.PostAsJsonAsync(BaseShipperPath, shipperModel);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
            Assert.NotNull(postResponse.Headers.Location);

            var shipperId = int.Parse(postResponse.Headers.Location.Segments.Last());
            var shipperOrderUrl = $"{BaseShipperPath}/{shipperId}/orders";

            // Attach order to shipper
            var shipperOrderModel = new ShipperOrderModel
            {
                ShipName = "Poseidon",
                ShippedDate = DateTimeOffset.Parse("2020-09-22T11:30:00+00:00")
            };

            var putResponse = await _client.PutAsJsonAsync($"{shipperOrderUrl}/{orderId}",
                                                           shipperOrderModel);

            Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
            var orderUpdated = await putResponse.Content.ReadAsAsync<OrderResponseModel>();
            Assert.Equal(2, orderUpdated.CustomerId);

            // Delete shipper
            var deleteResponse = await _client.DeleteAsync($"{BaseShipperPath}/{shipperId}");
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

            // Ensure order is detached from shipper
            var getResponse = await _client.GetAsync($"{BaseCustomerPath}/{customerId}/orders/{orderId}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var detachedOrder = await getResponse.Content.ReadAsAsync<OrderResponseModel>();
            Assert.Null(detachedOrder.ShipperId);
            Assert.Null(detachedOrder.ShippedDate);
            Assert.Null(detachedOrder.ShipName);

            // Delete order
            deleteResponse = await _client.DeleteAsync($"{customerOrderUri}/{orderId}");
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }
    }
}