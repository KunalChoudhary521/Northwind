using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using Northwind.API.Repositories;
using Northwind.API.Services;
using Northwind.Data.Entities;
using Xunit;

namespace Northwind.Tests.UnitTests.Services
{
    public class ShipperServiceTests
    {
        private readonly Mock<IRepository<Shipper>> _shipperRepository;
        private readonly Mock<IRepository<Order>> _orderRepository;
        private readonly IShipperService _shipperService;

        public ShipperServiceTests()
        {
            _shipperRepository = new Mock<IRepository<Shipper>>();
            _orderRepository = new Mock<IRepository<Order>>();
            var logger = new Mock<ILogger<ShipperService>>();

            _shipperService = new ShipperService(_shipperRepository.Object,
                                                 _orderRepository.Object,
                                                 logger.Object);
        }

        [Fact]
        public async Task ShipperWithOrders_Delete_DetachOrdersAndDeleteShipper()
        {
            var shipper = new Shipper();

            var orders = new []
            {
                new Order
                {
                    OrderId = 1,
                    ShipperId = 1,
                    ShippedDate = DateTimeOffset.UtcNow,
                    ShipName = "Name 1"
                },
                new Order
                {
                    OrderId = 2,
                    ShipperId = 1,
                    ShippedDate = DateTimeOffset.UtcNow,
                    ShipName = "Name 2"
                }
            };
            var mockOrders = orders.AsQueryable().BuildMock();

            _orderRepository.Setup(or => or.FindByCondition(It.IsAny<Expression<Func<Order, bool>>>()))
                            .Returns(mockOrders.Object);

            await _shipperService.Delete(shipper);

            _shipperRepository.Verify(sh => sh.Delete(shipper));

            var detachedOrders = await mockOrders.Object.ToArrayAsync();
            foreach (var order in detachedOrders)
            {
                Assert.Null(order.ShipperId);
                Assert.Null(order.ShippedDate);
                Assert.Null(order.ShipName);
            }
        }

        [Fact]
        public void ShipperWithOrders_DeleteOrder_DetachOrderFromShipper()
        {
            const int shipperId = 3;
            var order = new Order
            {
                ShipperId = 3,
                ShippedDate = DateTimeOffset.UtcNow,
                ShipName = "Test Name"
            };

            _shipperService.DeleteEntity(shipperId, order);

            Assert.Null(order.ShipperId);
            Assert.Null(order.ShippedDate);
            Assert.Null(order.ShipName);
        }
    }
}