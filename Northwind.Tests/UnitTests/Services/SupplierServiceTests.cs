using System;
using System.Collections.Generic;
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
    public class SupplierServiceTests
    {
        private readonly Mock<IRepository<Supplier>> _supplierRepository;
        private readonly Mock<IRepository<Location>> _locationRepository;
        private readonly Mock<IRepository<Product>> _productRepository;
        private readonly ISupplierService _supplierService;

        public SupplierServiceTests()
        {
            _supplierRepository = new Mock<IRepository<Supplier>>();
            _locationRepository = new Mock<IRepository<Location>>();
            _productRepository = new Mock<IRepository<Product>>();
            var logger = new Mock<ILogger<ISupplierService>>();

            _supplierService = new SupplierService(_supplierRepository.Object,
                                                   _locationRepository.Object,
                                                   _productRepository.Object,
                                                   logger.Object);
        }

        [Fact]
        public async Task SupplierWithProducts_Delete_DetachProductsDeleteSupplierAndLocation()
        {
            var supplier = new Supplier { Location = new Location() };

            var products = new []
            {
                new Product { ProductId = 1, SupplierId = 2 },
                new Product { ProductId = 2, SupplierId = 2 }
            };
            var mockProducts = products.AsQueryable().BuildMock();

            _productRepository.Setup(pr => pr.FindByCondition(It.IsAny<Expression<Func<Product, bool>>>()))
                              .Returns(mockProducts.Object);

            await _supplierService.Delete(supplier);

            _supplierRepository.Verify(s => s.Delete(supplier));
            _locationRepository.Verify(l => l.Delete(supplier.Location));

            var detachedProducts = await mockProducts.Object.ToArrayAsync();
            foreach (var product in detachedProducts)
                Assert.Null(product.SupplierId);
        }

        [Fact]
        public void SupplierIdAndProduct_AddProduct_AttachProductToSupplier()
        {
            const int supplierId = 5;
            var product = new Product();

            _supplierService.AddProduct(supplierId, product);

            Assert.Equal(5, product.SupplierId);
        }

        [Fact]
        public void SupplierIdAndProduct_UpdateProduct_AttachProductToSupplier()
        {
            const int supplierId = 7;
            var product = new Product();

            _supplierService.UpdateProduct(supplierId, product);

            Assert.Equal(7, product.SupplierId);
        }

        [Fact]
        public void SupplierIdAndProduct_DeleteProduct_DetachProductFromSupplier()
        {
            const int supplierId = 7;
            var product = new Product();

            _supplierService.DeleteProduct(supplierId, product);

            Assert.Null(product.SupplierId);
        }
    }
}