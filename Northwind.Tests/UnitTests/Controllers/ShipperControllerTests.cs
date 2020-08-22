using System;
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
    public class ShipperControllerTests
    {
        private readonly Mock<IShipperService> _shipperService;
        private readonly ShippersController _shipperController;

        public ShipperControllerTests()
        {
            var mapperConfig = new MapperConfiguration(config =>
            {
                config.AddProfiles(new Profile[] { new ShipperProfile(), new OrderProfile() });
            });

            var mapper = mapperConfig.CreateMapper();
            _shipperService = new Mock<IShipperService>();

            _shipperController = new ShippersController(_shipperService.Object, mapper);
        }

        [Fact]
        public async Task IdOfNonExistentShipper_GetShipper_ReturnNotFound()
        {
            const int shipperId = -1;

            var response = await _shipperController.GetShipper(shipperId);

            Assert.IsType<ActionResult<ShipperModel>>(response);
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task FailedToSaveNewShipper_AddShipper_ReturnBadRequest()
        {
            var shipperModel = new ShipperModel();

            var response = await _shipperController.AddShipper(shipperModel);

            Assert.IsType<ActionResult<ShipperModel>>(response);
            Assert.IsType<BadRequestResult>(response.Result);
            _shipperService.Verify(sh => sh.Add(It.IsAny<Shipper>()));
        }

        [Fact]
        public async Task ShipperModel_AddShipper_ReturnModelWithLocationInHeader()
        {
            var shipperModel = new ShipperModel();

            _shipperService.Setup(sh => sh.IsSavedToDb()).ReturnsAsync(true);

            var response = await _shipperController.AddShipper(shipperModel);

            Assert.IsType<ActionResult<ShipperModel>>(response);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(response.Result);

            Assert.IsType<ShipperModel>(createdAtActionResult.Value);
            Assert.Equal(StatusCodes.Status201Created, createdAtActionResult.StatusCode);
            Assert.Single(createdAtActionResult.RouteValues.Keys, "shipperId");
        }

        [Fact]
        public async Task IdOfNonExistentShipper_UpdateShipper_ShipperNotUpdated()
        {
            const int shipperId = -1;
            var shipperModel = new ShipperModel();

            var response = await _shipperController.UpdateShipper(shipperId, shipperModel);

            Assert.IsType<ActionResult<ShipperModel>>(response);
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task FailedToSaveUpdatedShipper_UpdateShipper_ReturnBadRequest()
        {
            const int shipperId = 7;

            _shipperService.Setup(sh => sh.GetById(shipperId)).ReturnsAsync(new Shipper());

            var response = await _shipperController.UpdateShipper(shipperId, new ShipperModel());

            Assert.IsType<ActionResult<ShipperModel>>(response);
            Assert.IsType<BadRequestResult>(response.Result);
            _shipperService.Verify(sh => sh.Update(It.IsAny<Shipper>()));
        }

        [Fact]
        public async Task IdOfNonExistentShipper_DeleteShipper_ShipperNotDeleted()
        {
            const int shipperId = -1;

            var response = await _shipperController.DeleteShipper(shipperId);

            Assert.IsType<NotFoundResult>(response);
        }

        [Fact]
        public async Task FailedToSaveDeletedShipper_DeleteShipper_ReturnBadRequest()
        {
            const int shipperId = 3;

            _shipperService.Setup(sh => sh.GetById(shipperId)).ReturnsAsync(new Shipper());

            var response = await _shipperController.DeleteShipper(shipperId);

            Assert.IsType<BadRequestResult>(response);
            _shipperService.Verify(sh => sh.Delete(It.IsAny<Shipper>()));
        }

        [Fact]
        public async Task IdOfNonExistentShipper_GetShipperOrders_ReturnNotFound()
        {
            const int shipperId = -1;

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _shipperController.GetShipperOrders(shipperId));

            Assert.Equal(StatusCodes.Status404NotFound, exception.Details.Status);
            Assert.Contains("shipper", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task IdOfNonExistentShipper_GetShipperOrder_ReturnNotFound()
        {
            const int shipperId = -1;
            const int orderId = 4;

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _shipperController.GetShipperOrder(shipperId, orderId));

            Assert.Equal(StatusCodes.Status404NotFound, exception.Details.Status);
            Assert.Contains("shipper", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task IdOfNonExistentOrder_GetShipperOrder_ReturnNotFound()
        {
            const int shipperId = 16;
            const int orderId = -1;

            _shipperService.Setup(sh => sh.GetById(shipperId)).ReturnsAsync(new Shipper());

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _shipperController.GetShipperOrder(shipperId, orderId));

            Assert.Equal(StatusCodes.Status404NotFound, exception.Details.Status);
            Assert.Contains("order", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task IdOfNonExistentShipper_UpdateShipperOrder_ReturnNotFound()
        {
            const int shipperId = -1;
            const int orderId = 4;
            var shipperOrderModel = new ShipperOrderModel();

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _shipperController.UpdateShipperOrder(shipperId, orderId, shipperOrderModel));

            Assert.Equal(StatusCodes.Status404NotFound, exception.Details.Status);
            Assert.Contains("shipper", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task IdOfNonExistentOrder_UpdateShipperOrder_ReturnNotFound()
        {
            const int shipperId = 3;
            const int orderId = -1;
            var shipperOrderModel = new ShipperOrderModel();

            _shipperService.Setup(sh => sh.GetById(shipperId)).ReturnsAsync(new Shipper());

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _shipperController.UpdateShipperOrder(shipperId, orderId, shipperOrderModel));

            Assert.Equal(StatusCodes.Status404NotFound, exception.Details.Status);
            Assert.Contains("order", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task FailedToSaveUpdatedOrder_UpdateShipperOrder_ReturnBadRequest()
        {
            const int shipperId = 3;
            const int orderId = 8;
            var shipperOrderModel = new ShipperOrderModel();

            _shipperService.Setup(sh => sh.GetById(shipperId)).ReturnsAsync(new Shipper());
            _shipperService.Setup(sh => sh.GetOrderById(orderId)).ReturnsAsync(new Order());

            var response = await _shipperController.UpdateShipperOrder(shipperId, orderId,
                                                                       shipperOrderModel);

            Assert.IsType<ActionResult<OrderResponseModel>>(response);
            Assert.IsType<BadRequestResult>(response.Result);
            _shipperService.Verify(sh => sh.UpdateEntity(shipperId, It.IsAny<Order>()));
        }

        [Fact]
        public async Task IdOfNonExistentShipper_DeleteShipperOrder_ReturnNotFound()
        {
            const int shipperId = -1;
            const int orderId = 4;

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _shipperController.DeleteShipperOrder(shipperId, orderId));

            Assert.Equal(StatusCodes.Status404NotFound, exception.Details.Status);
            Assert.Contains("shipper", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task IdOfNonExistentOrder_DeleteShipperOrder_ReturnNotFound()
        {
            const int shipperId = 6;
            const int orderId = -3;

            _shipperService.Setup(sh => sh.GetById(shipperId)).ReturnsAsync(new Shipper());

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _shipperController.DeleteShipperOrder(shipperId, orderId));

            Assert.Equal(StatusCodes.Status404NotFound, exception.Details.Status);
            Assert.Contains("order", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task FailedToSaveChangesAfterDelete_DeleteShipperOrder_ReturnBadRequest()
        {
            const int shipperId = 3;
            const int orderId = 8;

            _shipperService.Setup(sh => sh.GetById(shipperId)).ReturnsAsync(new Shipper());
            _shipperService.Setup(sh => sh.GetEntityById(shipperId, orderId))
                           .ReturnsAsync(new Order());

            var response = await _shipperController.DeleteShipperOrder(shipperId, orderId);

            Assert.IsType<ActionResult<OrderResponseModel>>(response);
            Assert.IsType<BadRequestResult>(response.Result);
            _shipperService.Verify(sh => sh.DeleteEntity(shipperId, It.IsAny<Order>()));
        }
    }
}