using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Northwind.API.Models.Orders;
using Northwind.API.Profiles;
using Northwind.Data.Entities;
using Xunit;

namespace Northwind.Tests.UnitTests.Profiles
{
    public class OrderProfileTests
    {
        private readonly IMapper _mapper;

        public OrderProfileTests()
        {
            var mapperConfig = new MapperConfiguration(config =>
            {
                config.AddProfile(new OrderProfile());
            });

            _mapper = mapperConfig.CreateMapper();
        }

        [Fact]
        public void OrderRequestModel_OrderRequestModelToOrder_ReturnOrder()
        {
            var orderRequestModel = new OrderRequestModel
            {
                RequiredDate = DateTimeOffset.Parse("2020-08-11T15:47:29+00:00"),
                OrderItems = new []
                {
                    new OrderItemRequestModel { ProductId = 1, Quantity = 2 },
                    new OrderItemRequestModel { ProductId = 27, Quantity = 5 },
                    new OrderItemRequestModel { ProductId = 35, Quantity = 15 }
                }
            };

            var order = _mapper.Map<Order>(orderRequestModel);

            Assert.Equal(0, order.OrderId);
            Assert.Equal(0, order.CustomerId);
            Assert.Equal(0, order.LocationId);
            Assert.Equal(decimal.Zero, order.Total);
            Assert.Equal(new DateTimeOffset(2020, 8, 11, 15, 47, 29, TimeSpan.Zero), order.RequiredDate);

            Assert.Null(order.Customer);
            Assert.Null(order.Employee);
            Assert.Null(order.EmployeeId);
            Assert.Null(order.Location);
            Assert.Null(order.Shipper);
            Assert.Null(order.ShipperId);

            var orderDetails = order.OrderDetails.ToArray();
            Assert.Equal(3, orderDetails.Length);

            Assert.Equal(1, orderDetails[0].ProductId);
            Assert.Equal(2, orderDetails[0].Quantity);

            Assert.Equal(27, orderDetails[1].ProductId);
            Assert.Equal(5, orderDetails[1].Quantity);

            Assert.Equal(35, orderDetails[2].ProductId);
            Assert.Equal(15, orderDetails[2].Quantity);
        }

        [Fact]
        public void Order_OrderToOrderResponseModel_ReturnOrderResponseModel()
        {
            var order = new Order
            {
                OrderId = 10,
                OrderDate = DateTimeOffset.Parse("2020-03-16T09:59:58-04:00"),
                RequiredDate = DateTimeOffset.Parse("2020-08-07T19:00:00+00:00"),
                Total = new decimal(13.40),
                OrderDetails = new List<OrderDetail>
                {
                    new OrderDetail
                    {
                        ProductId = 4,
                        Product = new Product { ProductName = "Product #1" },
                        Quantity = 10,
                        UnitPrice = decimal.Parse("10.80"),
                        Discount = decimal.Parse("1.60")
                    },
                    new OrderDetail
                    {
                        ProductId = 5,
                        Product = new Product { ProductName = "Product #2" },
                        Quantity = 3
                    }
                }
            };

            var orderResponseModel = _mapper.Map<OrderResponseModel>(order);

            Assert.Equal(10, orderResponseModel.OrderId);
            Assert.Equal(new decimal(13.40), orderResponseModel.Total);
            Assert.Equal(new DateTimeOffset(2020, 3, 16, 9, 59, 58, TimeSpan.FromHours(-4)),
                         orderResponseModel.OrderDate);
            Assert.Equal(new DateTimeOffset(2020, 8, 7, 19, 0, 0, TimeSpan.Zero), orderResponseModel.RequiredDate);
            Assert.Null(orderResponseModel.ShippedDate);

            var items = orderResponseModel.OrderItems.ToArray();
            Assert.Equal(2, items.Length);

            Assert.Equal(4, items[0].ProductId);
            Assert.Equal("Product #1", items[0].ProductName);
            Assert.Equal(10, items[0].Quantity);
            Assert.Equal(new decimal(10.80), items[0].UnitPrice);
            Assert.Equal(new decimal(1.60), items[0].Discount);

            Assert.Equal(5, items[1].ProductId);
            Assert.Equal("Product #2", items[1].ProductName);
            Assert.Equal(3, items[1].Quantity);
        }
    }
}