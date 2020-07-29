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
    public class CategoryServiceTests
    {
        private readonly Mock<IRepository<Category>> _categoryRepository;
        private readonly Mock<IRepository<Product>> _productRepository;
        private readonly ICategoryService _categoryService;

        public CategoryServiceTests()
        {
            _categoryRepository = new Mock<IRepository<Category>>();
            _productRepository = new Mock<IRepository<Product>>();
            var logger = new Mock<ILogger<CategoryService>>();

            _categoryService = new CategoryService(_categoryRepository.Object,
                                                   _productRepository.Object,
                                                   logger.Object);
        }

        [Fact]
        public async Task CategoryWithProducts_Delete_DetachProductsFromCategoryAndDeleteCategory()
        {
            var category = new Category
            {
                CategoryName = "Unit test category"
            };

            var products = new []
            {
                new Product { ProductId = 1, CategoryId = 1 },
                new Product { ProductId = 2, CategoryId = 1 }
            };
            var mockProducts = products.AsQueryable().BuildMock();

            _productRepository.Setup(pr => pr.FindByCondition(It.IsAny<Expression<Func<Product, bool>>>()))
                              .Returns(mockProducts.Object);

            await _categoryService.Delete(category);

            _categoryRepository.Verify(c => c.Delete(category));

            var detachedProducts = await mockProducts.Object.ToArrayAsync();
            foreach (var product in detachedProducts)
                Assert.Null(product.CategoryId);
        }

        [Fact]
        public void CategoryIdAndProduct_AddEntity_AttachProductToCategory()
        {
            const int categoryId = 2;
            var product = new Product();

            _categoryService.AddEntity(categoryId, product);

            Assert.Equal(2, product.CategoryId);
        }

        [Fact]
        public void CategoryIdAndProduct_UpdateEntity_AttachProductToCategory()
        {
            const int categoryId = 3;
            var product = new Product();

            _categoryService.UpdateEntity(categoryId, product);

            Assert.Equal(3, product.CategoryId);
        }

        [Fact]
        public void CategoryIdAndProduct_DeleteEntity_DetachProductFromCategory()
        {
            const int categoryId = 10;
            var product = new Product();

            _categoryService.DeleteEntity(categoryId, product);

            Assert.Null(product.CategoryId);
        }
    }
}