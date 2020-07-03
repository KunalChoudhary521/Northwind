using System.Collections.Generic;
using AutoMapper;
using Northwind.API.Models;
using Northwind.API.Profiles;
using Northwind.Data.Entities;
using Xunit;

namespace Northwind.Tests.UnitTests.Profiles
{
    public class CategoryProfileTests
    {
        private readonly IMapper _mapper;

        public CategoryProfileTests()
        {
            var mapperConfig = new MapperConfiguration(config =>
            {
                config.AddProfile(new CategoryProfile());
            });

            _mapper = mapperConfig.CreateMapper();
        }

        [Fact]
        public void Category_CategoryToCategoryModel_ReturnCategoryModel()
        {
            var category = new Category
            {
                CategoryId = 12,
                CategoryName = "Test beverages",
                Description = "Category for testing new beverages",
                Products = new List<Product>
                {
                    new Product {ProductName = "Chai Tea", UnitPrice = new decimal(3.49)},
                    new Product {ProductName = "Mojito Cocktail", UnitPrice = new decimal(15.49)}
                }
            };

            var categoryModel = _mapper.Map<CategoryModel>(category);

            Assert.Equal(12, categoryModel.CategoryId);
            Assert.Equal("Test beverages", categoryModel.CategoryName);
            Assert.Equal("Category for testing new beverages", categoryModel.Description);
        }

        [Fact]
        public void CategoryModelWithCategoryId_CategoryModelToCategory_IgnoreCategoryId()
        {
            var categoryModel = new CategoryModel
            {
                CategoryId = 12,
                CategoryName = "Test beverages",
                Description = "Category for testing new beverages"
            };

            var category = _mapper.Map<Category>(categoryModel);

            Assert.Equal(0, category.CategoryId);
            Assert.Equal("Test beverages", categoryModel.CategoryName);
            Assert.Equal("Category for testing new beverages", categoryModel.Description);
        }
    }
}