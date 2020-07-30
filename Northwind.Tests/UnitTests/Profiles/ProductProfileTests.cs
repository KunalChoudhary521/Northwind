using AutoMapper;
using Northwind.API.Models;
using Northwind.API.Profiles;
using Northwind.Data.Entities;
using Xunit;

namespace Northwind.Tests.UnitTests.Profiles
{
    public class ProductProfileTests
    {
        private readonly IMapper _mapper;

        public ProductProfileTests()
        {
            var mapperConfig = new MapperConfiguration(config =>
            {
                config.AddProfile(new ProductProfile());
            });

            _mapper = mapperConfig.CreateMapper();
        }

        [Fact]
        public void ProductModelWithProductId_ProductModelToProduct_IgnoreProductId()
        {
            var productModel = new ProductModel
            {
                ProductId = 27,
                ProductName = "Tofu"
            };

            var product = _mapper.Map<Product>(productModel);

            Assert.Equal(0, product.ProductId);
            Assert.Equal(0, product.SupplierId);
            Assert.Equal("Tofu", product.ProductName);
            Assert.Equal(0, product.UnitsInStock);
        }

        [Fact]
        public void CategoryIdIsNull_ProductModelToProduct_IdSetToNull()
        {
            var productModel = new ProductModel
            {
                ProductId = 14,
                ProductName = "Wine"
            };

            var product = _mapper.Map<Product>(productModel);

            Assert.Null(product.CategoryId);
        }
    }
}