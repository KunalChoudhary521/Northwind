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
    public class SupplierControllerTests
    {
        private readonly Mock<IService<Supplier>> _supplierService;
        private readonly SuppliersController _suppliersController;

        public SupplierControllerTests()
        {
            var mapperConfig = new MapperConfiguration(config =>
            {
                config.AddProfiles(new Profile[] { new SupplierProfile(), new LocationProfile() });
            });
            var mapper = mapperConfig.CreateMapper();
            _supplierService = new Mock<IService<Supplier>>();

            _suppliersController = new SuppliersController(_supplierService.Object, mapper);
        }

        [Fact]
        public async Task NonExistentSuppliers_GetSuppliers_ReturnNotFound()
        {
            _supplierService.Setup(s => s.GetAll())
                            .Returns(Task.FromResult<ICollection<Supplier>>(null));

            var response = await _suppliersController.GetSuppliers();

            Assert.IsType<ActionResult<SupplierModel[]>>(response);
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task IdOfNonexistentSupplier_GetSupplier_ReturnNotFound()
        {
            const int supplierId = -1;

            var response = await _suppliersController.GetSupplier(supplierId);

            Assert.IsType<ActionResult<SupplierModel>>(response);
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task FailedToAddSupplier_AddSupplier_ReturnBadRequest()
        {
            var supplierModel = new SupplierModel();

            var response = await _suppliersController.AddSupplier(supplierModel);

            Assert.IsType<ActionResult<SupplierModel>>(response);
            Assert.IsType<BadRequestResult>(response.Result);
            _supplierService.Verify(s => s.Add(It.IsAny<Supplier>()));
        }

        [Fact]
        public async Task SupplierModel_AddSupplier_ReturnModelWithLocationInHeader()
        {
            var supplierModel = new SupplierModel();

            _supplierService.Setup(s => s.Add(It.IsAny<Supplier>()))
                            .Returns(Task.FromResult(new Supplier { SupplierId = 30 }));

            var response = await _suppliersController.AddSupplier(supplierModel);

            Assert.IsType<ActionResult<SupplierModel>>(response);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(response.Result);

            Assert.IsType<SupplierModel>(createdAtActionResult.Value);
            Assert.Equal(StatusCodes.Status201Created, createdAtActionResult.StatusCode);
            Assert.Single(createdAtActionResult.RouteValues.Keys);
            Assert.Equal(30, createdAtActionResult.RouteValues["supplierId"]);
        }

        [Fact]
        public async Task IdOfNonexistentSupplier_UpdateSupplier_SupplierNotUpdated()
        {
            const int supplierId = -1;
            var supplierModel = new SupplierModel();

            var response = await _suppliersController.UpdateSupplier(supplierId, supplierModel);

            Assert.IsType<ActionResult<SupplierModel>>(response);
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task FailedToUpdateSupplier_UpdateSupplier_ReturnBadRequest()
        {
            const int supplierId = 40;
            var supplierModel = new SupplierModel();

            _supplierService.Setup(s => s.GetById(supplierId))
                            .Returns(Task.FromResult(new Supplier()));
            _supplierService.Setup(s => s.IsSavedToDb()).Returns(Task.FromResult(false));

            var response = await _suppliersController.UpdateSupplier(supplierId, supplierModel);

            Assert.IsType<ActionResult<SupplierModel>>(response);
            Assert.IsType<BadRequestResult>(response.Result);
            _supplierService.Verify(s => s.Update(It.IsAny<Supplier>()));
        }

        [Fact]
        public async Task IdOfNonexistentSupplier_DeleteSupplier_SupplierNotDeleted()
        {
            const int supplierId = -1;

            var response = await _suppliersController.DeleteSupplier(supplierId);

            Assert.IsType<NotFoundResult>(response);
        }
    }
}