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
    public class CategoryControllerTests
    {
        private readonly Mock<ICategoryService> _categoryService;
        private readonly CategoriesController _categoriesController;

        public CategoryControllerTests()
        {
            var mapperConfig = new MapperConfiguration(config => { config.AddProfile(new CategoryProfile()); });
            var mapper = mapperConfig.CreateMapper();
            _categoryService = new Mock<ICategoryService>();

            _categoriesController = new CategoriesController(_categoryService.Object, mapper);
        }

        [Fact]
        public async Task NonExistentCategories_GetCategories_ReturnNotFound()
        {
            _categoryService.Setup(s => s.GetAllCategories()).Returns(Task.FromResult<Category[]>(null));

            var response = await _categoriesController.GetCategories();

            Assert.IsType<ActionResult<CategoryModel[]>>(response);
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task PreExistingCategories_GetCategories_ReturnCategories()
        {
            _categoryService.Setup(s => s.GetAllCategories()).Returns(Task.FromResult(new[] { new Category() }));

            var response = await _categoriesController.GetCategories();

            Assert.IsType<ActionResult<CategoryModel[]>>(response);
            Assert.IsType<CategoryModel[]>(response.Value);
            Assert.Single(response.Value);
        }

        [Fact]
        public async Task IdOfNonexistentCategory_GetCategory_ReturnNotFound()
        {
            const int categoryId = -1;

            _categoryService.Setup(s => s.GetCategoryById(categoryId)).Returns(Task.FromResult<Category>(null));

            var response = await _categoriesController.GetCategory(categoryId);

            Assert.IsType<ActionResult<CategoryModel>>(response);
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task CategoryId_GetCategory_ReturnCategory()
        {
            const int categoryId = 5;

            _categoryService.Setup(s => s.GetCategoryById(categoryId)).Returns(Task.FromResult(new Category()));

            var response = await _categoriesController.GetCategory(categoryId);

            Assert.IsType<ActionResult<CategoryModel>>(response);
            Assert.IsType<CategoryModel>(response.Value);
        }

        [Fact]
        public async Task FailedToSaveCategory_AddCategory_ReturnBadRequest()
        {
            var categoryModel = new CategoryModel { CategoryName = "Failed to save category" };

            var response = await _categoriesController.AddCategory(categoryModel);

            Assert.IsType<ActionResult<CategoryModel>>(response);
            Assert.IsType<BadRequestResult>(response.Result);
            _categoryService.Verify(s => s.IsSavedToDb());
        }

        [Fact]
        public async Task ExistingCategory_AddCategory_ReturnBadRequest()
        {
            var categoryModel = new CategoryModel { CategoryName = "Existing Category" };

            _categoryService.Setup(s => s.GetCategoryByName(It.IsAny<string>()))
                            .Returns(Task.FromResult(new Category()));

            var response = await _categoriesController.AddCategory(categoryModel);

            Assert.IsType<ActionResult<CategoryModel>>(response);
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }

        [Fact]
        public async Task CategoryModel_AddCategory_ReturnModelWithLocationInHeader()
        {
            var categoryModel = new CategoryModel { CategoryName = "New Category" };

            var category = new Category { CategoryId = 3, CategoryName = "New Category" };
            _categoryService.Setup(s => s.IsSavedToDb()).Returns(Task.FromResult(true));
            _categoryService.SetupSequence(s => s.GetCategoryByName(It.IsAny<string>()))
                            .Returns(Task.FromResult<Category>(null))
                            .Returns(Task.FromResult(category));

            var response = await _categoriesController.AddCategory(categoryModel);

            Assert.IsType<ActionResult<CategoryModel>>(response);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(response.Result);

            Assert.IsType<CategoryModel>(createdAtActionResult.Value);
            Assert.Equal(StatusCodes.Status201Created, createdAtActionResult.StatusCode);
            Assert.Single(createdAtActionResult.RouteValues.Keys);
            Assert.Equal(3, createdAtActionResult.RouteValues["categoryId"]);
        }

        [Fact]
        public async Task IdOfNonexistentCategory_UpdateCategory_CategoryNotUpdated()
        {
            const int categoryId = -1;
            var categoryModel = new CategoryModel { Description = "New Description" };

            _categoryService.Setup(s => s.GetCategoryById(categoryId)).Returns(Task.FromResult<Category>(null));

            var response = await _categoriesController.UpdateCategory(categoryId, categoryModel);

            Assert.IsType<ActionResult<CategoryModel>>(response);
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task CategoryIdAndCategoryModel_UpdateCategory_ReturnUpdatedCategory()
        {
            const int categoryId = 5;
            var categoryModel = new CategoryModel { CategoryName = "New Name", Description = "New Description" };

            var oldCategory = new Category
            {
                CategoryId = 5,
                CategoryName = "Old Name",
                Description = "Old Description"
            };
            _categoryService.Setup(s => s.GetCategoryById(categoryId)).Returns(Task.FromResult(oldCategory));
            _categoryService.Setup(s => s.IsSavedToDb()).Returns(Task.FromResult(true));

            var response = await _categoriesController.UpdateCategory(categoryId, categoryModel);

            Assert.IsType<ActionResult<CategoryModel>>(response);
            var okObjectResult = Assert.IsType<OkObjectResult>(response.Result);

            var updateCategory = Assert.IsType<CategoryModel>(okObjectResult.Value);
            Assert.Equal(5, updateCategory.CategoryId);
            Assert.Equal("New Name", updateCategory.CategoryName);
            Assert.Equal("New Description", updateCategory.Description);
        }

        [Fact]
        public async Task IdOfNonexistentCategory_DeleteCategory_CategoryNotDeleted()
        {
            const int categoryId = -1;

            _categoryService.Setup(s => s.GetCategoryById(categoryId)).Returns(Task.FromResult<Category>(null));

            var response = await _categoriesController.DeleteCategory(categoryId);

            Assert.IsType<NotFoundResult>(response);
        }

        [Fact]
        public async Task CategoryId_DeleteCategory_CategoryIsDeleted()
        {
            //TODO: Add a test to check if related products are deleted

            const int categoryId = 4;
            var existingCategory = new Category { CategoryId = 4, CategoryName = "Delete Category" };
            _categoryService.Setup(s => s.GetCategoryById(categoryId)).Returns(Task.FromResult(existingCategory));
            _categoryService.Setup(s => s.IsSavedToDb()).Returns(Task.FromResult(true));

            var response = await _categoriesController.DeleteCategory(categoryId);

            Assert.IsType<OkObjectResult>(response);
        }
    }
}