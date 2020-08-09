using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using Northwind.API.Repositories;
using Northwind.API.Services;
using Northwind.Data.Entities;
using Xunit;

namespace Northwind.Tests.UnitTests.Services
{
    public class CustomerServiceTests
    {
        private readonly Mock<IRepository<Location>> _locationRepository;
        private readonly Mock<IRepository<Customer>> _customerRepository;
        private readonly Mock<IRepository<Order>> _orderRepository;
        private readonly Mock<IRepository<Product>> _productRepository;
        private readonly ICustomerService _customerService;

        public CustomerServiceTests()
        {
            _customerRepository = new Mock<IRepository<Customer>>();
            _locationRepository = new Mock<IRepository<Location>>();
            _orderRepository = new Mock<IRepository<Order>>();
            _productRepository = new Mock<IRepository<Product>>();
            var logger = new Mock<ILogger<CustomerService>>();

            _customerService = new CustomerService(_customerRepository.Object,
                                                   _locationRepository.Object,
                                                   _orderRepository.Object,
                                                   _productRepository.Object,
                                                   logger.Object);
        }

        [Fact]
        public async Task CustomerWithLocation_Delete_DeleteCustomerAndLocation()
        {
            var customer = new Customer { Location = new Location() };

            var mockOrders = new List<Order>().AsQueryable().BuildMock();
            _orderRepository.Setup(or => or.FindByCondition(It.IsAny<Expression<Func<Order, bool>>>()))
                            .Returns(mockOrders.Object);

            await _customerService.Delete(customer);

            _customerRepository.Verify(c => c.Delete(customer));
            _locationRepository.Verify(l => l.Delete(customer.Location));
        }

        [Fact]
        public async Task CustomerWithOrdersAndLocation_Delete_DeleteCustomerAndOrdersAndLocation()
        {
            var customer = new Customer { Location = new Location() };

            var orders = new List<Order>
            {
                new Order { OrderId = 1},
                new Order { OrderId = 2}
            };
            var mockOrders = orders.AsQueryable().BuildMock();
            _orderRepository.Setup(or => or.FindByCondition(It.IsAny<Expression<Func<Order, bool>>>()))
                            .Returns(mockOrders.Object);

            await _customerService.Delete(customer);

            _customerRepository.Verify(c => c.Delete(customer));
            _locationRepository.Verify(l => l.Delete(customer.Location));
            _orderRepository.Verify(o => o.Delete(It.IsAny<Order>()), Times.Exactly(2));
        }

        [Fact]
        public async Task NegativeProductQuantity_AddEntity_ThrowException()
        {
            var customer = new Customer();
            var order = new Order
            {
                OrderDetails = new List<OrderDetail>
                {
                    new OrderDetail { Quantity = -1}
                }
            };

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _customerService.AddEntity(customer, order));

            Assert.Equal(StatusCodes.Status400BadRequest, exception.Details.Status);
            Assert.Contains("product", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task IdsOfNonExistentProducts_AddEntity_ThrowException()
        {
            var customer = new Customer { Location = new Location() };
            var order = new Order
            {
                OrderDetails = new []
                {
                    new OrderDetail { ProductId = 1, Quantity = 1 },
                    new OrderDetail { ProductId = 2, Quantity = 1 },
                    new OrderDetail { ProductId = 3, Quantity = 1 }
                }
            };

            var dbProducts = new [] { new Product { ProductId = 1, Discontinued = 0 } };
            var mockProducts = dbProducts.AsQueryable().BuildMock();
            var defaultProducts = new[] { new Product() }.AsQueryable().BuildMock();
            _productRepository.SetupSequence(pr => pr.FindByCondition(It.IsAny<Expression<Func<Product, bool>>>()))
                              .Returns(mockProducts.Object)
                              .Returns(defaultProducts.Object)
                              .Returns(defaultProducts.Object);

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _customerService.AddEntity(customer, order));

            Assert.Equal(StatusCodes.Status400BadRequest, exception.Details.Status);
            Assert.Contains("2,3", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }
    }
}