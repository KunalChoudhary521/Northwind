using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Northwind.API.Controllers;
using Northwind.API.Models;
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
                                                   new LocationProfile() });
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
    }
}