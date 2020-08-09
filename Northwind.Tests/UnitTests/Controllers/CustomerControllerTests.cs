using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Northwind.API.Controllers;
using Northwind.API.Models;
using Northwind.API.Models.Orders;
using Northwind.API.Profiles;
using Northwind.API.Services;
using Northwind.Data.Entities;
using Xunit;

namespace Northwind.Tests.UnitTests.Controllers
{
    public class CustomerControllerTests
    {
        private readonly Mock<ICustomerService> _customerService;
        private readonly CustomersController _customerController;

        public CustomerControllerTests()
        {
            var mapperConfig = new MapperConfiguration(config =>
            {
                config.AddProfiles(new Profile[] { new CustomerProfile(),
                                                   new LocationProfile(),
                                                   new OrderProfile() });
            });
            var mapper = mapperConfig.CreateMapper();
            _customerService = new Mock<ICustomerService>();

            _customerController = new CustomersController(_customerService.Object, mapper);
        }

        [Fact]
        public async Task NonExistentCustomers_GetCustomers_ReturnNotFound()
        {
            _customerService.Setup(c => c.GetAll())
                            .Returns(Task.FromResult<ICollection<Customer>>(null));

            var response = await _customerController.GetCustomers();

            Assert.IsType<ActionResult<CustomerModel[]>>(response);
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task IdOfNonExistentCustomer_GetCustomer_ReturnNotFound()
        {
            const int customerId = -1;

            var response = await _customerController.GetCustomer(customerId);

            Assert.IsType<ActionResult<CustomerModel>>(response);
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task FailedToSaveNewCustomers_AddCustomer_ReturnBadRequest()
        {
            var customerModel = new CustomerModel();

            var response = await _customerController.AddCustomer(customerModel);

            Assert.IsType<ActionResult<CustomerModel>>(response);
            Assert.IsType<BadRequestResult>(response.Result);
            _customerService.Verify(c => c.Add(It.IsAny<Customer>()));
        }

        [Fact]
        public async Task CustomerModel_AddCustomer_ReturnModelWithLocationInHeader()
        {
            var customerModel = new CustomerModel();

            _customerService.Setup(c => c.IsSavedToDb()).ReturnsAsync(true);

            var response = await _customerController.AddCustomer(customerModel);

            Assert.IsType<ActionResult<CustomerModel>>(response);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(response.Result);

            Assert.IsType<CustomerModel>(createdAtActionResult.Value);
            Assert.Equal(StatusCodes.Status201Created, createdAtActionResult.StatusCode);
            Assert.Single(createdAtActionResult.RouteValues.Keys, "customerId");
        }

        [Fact]
        public async Task IdOfNonExistentCustomer_UpdateCustomer_CustomerNotUpdated()
        {
            const int customerId = -1;
            var customerModel = new CustomerModel();

            var response = await _customerController.UpdateCustomer(customerId, customerModel);

            Assert.IsType<ActionResult<CustomerModel>>(response);
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task FailedToSaveUpdatedCustomer_UpdateCustomer_ReturnBadRequest()
        {
            const int customerId = 14;

            _customerService.Setup(c => c.GetById(customerId)).ReturnsAsync(new Customer());

            var response = await _customerController.UpdateCustomer(customerId, new CustomerModel());

            Assert.IsType<ActionResult<CustomerModel>>(response);
            Assert.IsType<BadRequestResult>(response.Result);
            _customerService.Verify(c => c.Update(It.IsAny<Customer>()));
        }

        [Fact]
        public async Task IdOfNonExistentCustomer_DeleteCustomer_CustomerNotDeleted()
        {
            const int customerId = -1;

            var response = await _customerController.DeleteCustomer(customerId);

            Assert.IsType<NotFoundResult>(response);
        }

        [Fact]
        public async Task FailedToSaveDeletedCustomer_DeleteCustomer_ReturnBadRequest()
        {
            const int customerId = 5;

            _customerService.Setup(c => c.GetById(customerId)).ReturnsAsync(new Customer());

            var response = await _customerController.DeleteCustomer(customerId);

            Assert.IsType<BadRequestResult>(response);
            _customerService.Verify(c => c.Delete(It.IsAny<Customer>()));
        }

        [Fact]
        public async Task IdOfNonexistentCustomer_GetCustomerOrders_ReturnNotFound()
        {
            const int customerId = -1;

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _customerController.GetCustomerOrders(customerId));

            Assert.Equal(StatusCodes.Status404NotFound, exception.Details.Status);
            Assert.Contains("customer", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task IdOfNonexistentCustomer_GetCustomerOrder_ReturnNotFound()
        {
            const int customerId = -1;
            const int orderId = 6;

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _customerController.GetCustomerOrder(customerId, orderId));

            Assert.Equal(StatusCodes.Status404NotFound, exception.Details.Status);
            Assert.Contains("customer", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task IdOfNonexistentOrder_GetCustomerOrder_ReturnNotFound()
        {
            const int customerId = 2;
            const int orderId = -5;

            _customerService.Setup(c => c.GetById(customerId)).ReturnsAsync(new Customer());

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _customerController.GetCustomerOrder(customerId, orderId));

            Assert.Equal(StatusCodes.Status404NotFound, exception.Details.Status);
            Assert.Contains("order", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task IdOfNonexistentCustomer_AddCustomerOrder_ReturnNotFound()
        {
            const int customerId = -1;
            var orderModel = new OrderRequestModel();

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _customerController.AddCustomerOrder(customerId, orderModel));

            Assert.Equal(StatusCodes.Status404NotFound, exception.Details.Status);
            Assert.Contains("customer", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task FailedToSaveNewOrder_AddCustomerOrder_ReturnBadRequest()
        {
            const int customerId = 2;
            var orderModel = new OrderRequestModel();

            _customerService.Setup(c => c.GetById(customerId)).ReturnsAsync(new Customer());

            var response = await _customerController.AddCustomerOrder(customerId, orderModel);

            Assert.IsType<ActionResult<OrderResponseModel>>(response);
            Assert.IsType<BadRequestResult>(response.Result);
            _customerService.Verify(c => c.AddEntity(It.IsAny<Customer>(), It.IsAny<Order>()));
        }

        [Fact]
        public async Task OrderRequestModel_AddCustomerOrder_ResponseContainsLocationInHeader()
        {
            const int customerId = 2;
            var orderModel = new OrderRequestModel();

            _customerService.Setup(c => c.GetById(customerId)).ReturnsAsync(new Customer());
            _customerService.Setup(c => c.IsSavedToDb()).ReturnsAsync(true);

            var response = await _customerController.AddCustomerOrder(customerId, orderModel);

            Assert.IsType<ActionResult<OrderResponseModel>>(response);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(response.Result);

            Assert.IsType<OrderResponseModel>(createdAtActionResult.Value);
            Assert.Equal(StatusCodes.Status201Created, createdAtActionResult.StatusCode);
            Assert.Equal(2, createdAtActionResult.RouteValues.Keys.Count);
            Assert.Contains("customerId", createdAtActionResult.RouteValues.Keys);
            Assert.Contains("orderId", createdAtActionResult.RouteValues.Keys);
        }

        [Fact]
        public async Task IdOfNonexistentCustomer_UpdateCustomerOrder_ReturnNotFound()
        {
            const int customerId = -1;
            const int orderId = 7;
            var orderModel = new OrderModelBase();

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _customerController.UpdateCustomerOrder(customerId, orderId, orderModel));

            Assert.Equal(StatusCodes.Status404NotFound, exception.Details.Status);
            Assert.Contains("customer", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task IdOfNonexistentOrder_UpdateCustomerOrder_ReturnNotFound()
        {
            const int customerId = 14;
            const int orderId = -1;
            var orderModel = new OrderModelBase();

            _customerService.Setup(c => c.GetById(customerId)).ReturnsAsync(new Customer());

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _customerController.UpdateCustomerOrder(customerId, orderId, orderModel));

            Assert.Equal(StatusCodes.Status404NotFound, exception.Details.Status);
            Assert.Contains("order", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task OrderWithShippedDate_UpdateCustomerOrder_ReturnBadRequest()
        {
            const int customerId = 14;
            const int orderId = 5;
            var orderModel = new OrderModelBase { RequiredDate = DateTimeOffset.Now };

            var mockOrder = new Order { ShippedDate = DateTimeOffset.UtcNow };
            _customerService.Setup(c => c.GetById(customerId)).ReturnsAsync(new Customer());
            _customerService.Setup(c => c.GetEntityById(customerId, orderId)).ReturnsAsync(mockOrder);

            var response = await _customerController.UpdateCustomerOrder(customerId, orderId, orderModel);

            Assert.IsType<ActionResult<OrderResponseModel>>(response);
            var result = Assert.IsType<BadRequestObjectResult>(response.Result);
            Assert.NotNull(result.Value);
        }

        [Fact]
        public async Task FailedToSaveUpdatedOrder_UpdateCustomerOrder_ReturnBadRequest()
        {
            const int customerId = 4;
            const int orderId = 2;
            var orderModel = new OrderModelBase { RequiredDate = DateTimeOffset.Now };

            _customerService.Setup(c => c.GetById(customerId)).ReturnsAsync(new Customer());
            _customerService.Setup(c => c.GetEntityById(customerId, orderId))
                            .ReturnsAsync(new Order());

            var response = await _customerController.UpdateCustomerOrder(customerId, orderId, orderModel);

            Assert.IsType<ActionResult<OrderResponseModel>>(response);
            Assert.IsType<BadRequestResult>(response.Result);
            _customerService.Verify(c => c.UpdateEntity(customerId, It.IsAny<Order>()));
        }

        [Fact]
        public async Task IdOfNonexistentCustomer_DeleteCustomerOrder_ReturnNotFound()
        {
            const int customerId = -1;
            const int orderId = 8;

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _customerController.DeleteCustomerOrder(customerId, orderId));

            Assert.Equal(StatusCodes.Status404NotFound, exception.Details.Status);
            Assert.Contains("customer", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task IdOfNonexistentOrder_DeleteCustomerOrder_ReturnNotFound()
        {
            const int customerId = 4;
            const int orderId = -1;

            _customerService.Setup(c => c.GetById(customerId)).ReturnsAsync(new Customer());

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _customerController.DeleteCustomerOrder(customerId, orderId));

            Assert.Equal(StatusCodes.Status404NotFound, exception.Details.Status);
            Assert.Contains("order", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task FailedToSaveChangesAfterDelete_DeleteCustomerOrder_ReturnBadRequest()
        {
            const int customerId = 4;
            const int orderId = 2;

            _customerService.Setup(c => c.GetById(customerId)).ReturnsAsync(new Customer());
            _customerService.Setup(c => c.GetEntityById(customerId, orderId))
                            .ReturnsAsync(new Order());

            var response = await _customerController.DeleteCustomerOrder(customerId, orderId);

            Assert.IsType<BadRequestResult>(response);
            _customerService.Verify(c => c.DeleteEntity(customerId, It.IsAny<Order>()));
        }
    }
}