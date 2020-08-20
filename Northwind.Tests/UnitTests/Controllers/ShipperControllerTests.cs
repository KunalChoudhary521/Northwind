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
    public class ShipperControllerTests
    {
        private readonly Mock<IShipperService> _shipperService;
        private readonly ShippersController _shipperController;

        public ShipperControllerTests()
        {
            var mapperConfig = new MapperConfiguration(config =>
            {
                config.AddProfiles(new Profile[] { new ShipperProfile() });
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
    }
}