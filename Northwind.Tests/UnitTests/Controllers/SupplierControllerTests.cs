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
using Northwind.API.Profiles;
using Northwind.API.Services;
using Northwind.Data.Entities;
using Xunit;

namespace Northwind.Tests.UnitTests.Controllers
{
    public class SupplierControllerTests
    {
        private readonly Mock<ISupplierService> _supplierService;
        private readonly SuppliersController _suppliersController;

        public SupplierControllerTests()
        {
            var mapperConfig = new MapperConfiguration(config =>
            {
                config.AddProfiles(new Profile[] { new SupplierProfile(),
                                                   new LocationProfile(),
                                                   new ProductProfile() });
            });
            var mapper = mapperConfig.CreateMapper();
            _supplierService = new Mock<ISupplierService>();

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
        public async Task FailedToSaveNewSupplier_AddSupplier_ReturnBadRequest()
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

            _supplierService.Setup(s => s.IsSavedToDb()).Returns(Task.FromResult(true));

            var response = await _suppliersController.AddSupplier(supplierModel);

            Assert.IsType<ActionResult<SupplierModel>>(response);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(response.Result);

            Assert.IsType<SupplierModel>(createdAtActionResult.Value);
            Assert.Equal(StatusCodes.Status201Created, createdAtActionResult.StatusCode);
            Assert.Single(createdAtActionResult.RouteValues.Keys, "supplierId");
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
        public async Task FailedToSaveUpdateSupplier_UpdateSupplier_ReturnBadRequest()
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

        [Fact]
        public async Task IdOfNonexistentSupplier_GetSupplierProducts_ReturnNotFound()
        {
            const int supplierId = -1;

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _suppliersController.GetSupplierProducts(supplierId));

            Assert.Equal(StatusCodes.Status404NotFound, exception.Details.Status);
            Assert.Contains("supplier", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task IdOfNonexistentSupplier_GetSupplierProduct_ReturnNotFound()
        {
            const int supplierId = -1;
            const int productId = 10;

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _suppliersController.GetSupplierProduct(supplierId, productId));

            Assert.Equal(StatusCodes.Status404NotFound, exception.Details.Status);
            Assert.Contains("supplier", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task IdOfNonexistentProduct_GetSupplierProduct_ReturnNotFound()
        {
            const int supplierId = 10;
            const int productId = -1;

            _supplierService.Setup(s => s.GetById(supplierId))
                            .Returns(Task.FromResult(new Supplier()));

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _suppliersController.GetSupplierProduct(supplierId, productId));

            Assert.Equal(StatusCodes.Status404NotFound, exception.Details.Status);
            Assert.Contains("product", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task IdOfNonexistentSupplier_AddSupplierProduct_ReturnNotFound()
        {
            const int supplierId = -1;
            var productModel = new ProductModel();

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _suppliersController.AddSupplierProduct(supplierId, productModel));

            Assert.Equal(StatusCodes.Status404NotFound, exception.Details.Status);
            Assert.Contains("supplier", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task FailedToSaveNewProduct_AddSupplierProduct_ReturnBadRequest()
        {
            const int supplierId = 10;
            var productModel = new ProductModel();

            _supplierService.Setup(s => s.GetById(supplierId))
                            .Returns(Task.FromResult(new Supplier()));

            var response = await _suppliersController.AddSupplierProduct(supplierId, productModel);

            Assert.IsType<ActionResult<ProductModel>>(response);
            Assert.IsType<BadRequestResult>(response.Result);
            _supplierService.Verify(s => s.AddProduct(supplierId, It.IsAny<Product>()));
        }

        [Fact]
        public async Task ProductModel_AddSupplierProduct_ResponseContainsLocationInHeader()
        {
            const int supplierId = 10;
            var productModel = new ProductModel();

            _supplierService.Setup(s => s.GetById(supplierId))
                            .Returns(Task.FromResult(new Supplier()));
            _supplierService.Setup(s => s.IsSavedToDb()).Returns(Task.FromResult(true));

            var response = await _suppliersController.AddSupplierProduct(supplierId, productModel);

            Assert.IsType<ActionResult<ProductModel>>(response);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(response.Result);

            Assert.IsType<ProductModel>(createdAtActionResult.Value);
            Assert.Equal(StatusCodes.Status201Created, createdAtActionResult.StatusCode);
            Assert.Equal(2, createdAtActionResult.RouteValues.Keys.Count);
            Assert.Contains("supplierId", createdAtActionResult.RouteValues.Keys);
            Assert.Contains("productId", createdAtActionResult.RouteValues.Keys);
        }

        [Fact]
        public async Task IdOfNonexistentSupplier_UpdateSupplierProduct_ReturnNotFound()
        {
            const int supplierId = -1;
            const int productId = 10;
            var productModel = new ProductModel();

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _suppliersController.UpdateSupplierProduct(supplierId, productId, productModel));

            Assert.Equal(StatusCodes.Status404NotFound, exception.Details.Status);
            Assert.Contains("supplier", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task IdOfNonexistentProduct_UpdateSupplierProduct_ReturnNotFound()
        {
            const int supplierId = 10;
            const int productId = -1;
            var productModel = new ProductModel();

            _supplierService.Setup(s => s.GetById(supplierId))
                            .Returns(Task.FromResult(new Supplier()));

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _suppliersController.UpdateSupplierProduct(supplierId, productId, productModel));

            Assert.Equal(StatusCodes.Status404NotFound, exception.Details.Status);
            Assert.Contains("product", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task FailedToSaveUpdatedProduct_UpdateSupplierProduct_ReturnBadRequest()
        {
            const int supplierId = 10;
            const int productId = 2;
            var productModel = new ProductModel();

            _supplierService.Setup(s => s.GetById(supplierId))
                            .Returns(Task.FromResult(new Supplier()));
            _supplierService.Setup(s => s.GetProductById(supplierId, productId))
                            .Returns(Task.FromResult(new Product()));

            var response = await _suppliersController.UpdateSupplierProduct(supplierId,
                                                                            productId,
                                                                            productModel);

            Assert.IsType<ActionResult<ProductModel>>(response);
            Assert.IsType<BadRequestResult>(response.Result);
            _supplierService.Verify(s => s.UpdateProduct(supplierId, It.IsAny<Product>()));
        }

        [Fact]
        public async Task IdOfNonexistentSupplier_DeleteSupplierProduct_ReturnNotFound()
        {
            const int supplierId = -1;
            const int productId = 10;

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _suppliersController.DeleteSupplierProduct(supplierId, productId));

            Assert.Equal(StatusCodes.Status404NotFound, exception.Details.Status);
            Assert.Contains("supplier", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task IdOfNonexistentProduct_DeleteSupplierProduct_ReturnNotFound()
        {
            const int supplierId = 10;
            const int productId = -1;

            _supplierService.Setup(s => s.GetById(supplierId))
                            .Returns(Task.FromResult(new Supplier()));

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _suppliersController.DeleteSupplierProduct(supplierId, productId));

            Assert.Equal(StatusCodes.Status404NotFound, exception.Details.Status);
            Assert.Contains("product", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task FailedToSaveDeletedProduct_DeleteSupplierProduct_ReturnBadRequest()
        {
            const int supplierId = 10;
            const int productId = 2;

            _supplierService.Setup(s => s.GetById(supplierId))
                            .Returns(Task.FromResult(new Supplier()));
            _supplierService.Setup(s => s.GetProductById(supplierId, productId))
                            .Returns(Task.FromResult(new Product()));

            var response = await _suppliersController.DeleteSupplierProduct(supplierId, productId);

            Assert.IsType<ActionResult<ProductModel>>(response);
            Assert.IsType<BadRequestResult>(response.Result);
            _supplierService.Verify(s => s.DeleteProduct(supplierId, It.IsAny<Product>()));
        }
    }
}