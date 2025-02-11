﻿using System;
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
    public class CategoryControllerTests
    {
        private readonly Mock<ICategoryService> _categoryService;
        private readonly CategoriesController _categoriesController;

        public CategoryControllerTests()
        {
            var mapperConfig = new MapperConfiguration(config =>
            {
                config.AddProfiles(new Profile[] { new CategoryProfile(),
                                                   new ProductProfile() });
            });
            var mapper = mapperConfig.CreateMapper();
            _categoryService = new Mock<ICategoryService>();

            _categoriesController = new CategoriesController(_categoryService.Object, mapper);
        }

        [Fact]
        public async Task NonExistentCategories_GetCategories_ReturnNotFound()
        {
            _categoryService.Setup(s => s.GetAll())
                            .Returns(Task.FromResult<ICollection<Category>>(null));

            var response = await _categoriesController.GetCategories();

            Assert.IsType<ActionResult<CategoryModel[]>>(response);
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task IdOfNonexistentCategory_GetCategory_ReturnNotFound()
        {
            const int categoryId = -1;

            var response = await _categoriesController.GetCategory(categoryId);

            Assert.IsType<ActionResult<CategoryModel>>(response);
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task CategoryId_GetCategory_ReturnCategory()
        {
            const int categoryId = 5;

            _categoryService.Setup(s => s.GetById(categoryId)).ReturnsAsync(new Category());

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
                            .ReturnsAsync(new Category());

            var response = await _categoriesController.AddCategory(categoryModel);

            Assert.IsType<ActionResult<CategoryModel>>(response);
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }

        [Fact]
        public async Task CategoryModel_AddCategory_ReturnModelWithLocationInHeader()
        {
            var categoryModel = new CategoryModel { CategoryName = "New Category" };

            var category = new Category { CategoryId = 3, CategoryName = "New Category" };
            _categoryService.Setup(s => s.IsSavedToDb()).ReturnsAsync(true);
            _categoryService.SetupSequence(s => s.GetCategoryByName(It.IsAny<string>()))
                            .Returns(Task.FromResult<Category>(null))
                            .ReturnsAsync(category);

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

            var response = await _categoriesController.UpdateCategory(categoryId, categoryModel);

            Assert.IsType<ActionResult<CategoryModel>>(response);
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task CategoryIdAndCategoryModel_UpdateCategory_ReturnUpdatedCategory()
        {
            const int categoryId = 5;
            var categoryModel = new CategoryModel
            {
                CategoryName = "New Name",
                Description = "New Description"
            };

            var oldCategory = new Category
            {
                CategoryId = 5,
                CategoryName = "Old Name",
                Description = "Old Description"
            };

            _categoryService.Setup(s => s.GetById(categoryId)).ReturnsAsync(oldCategory);
            _categoryService.Setup(s => s.IsSavedToDb()).ReturnsAsync(true);

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

            var response = await _categoriesController.DeleteCategory(categoryId);

            Assert.IsType<NotFoundResult>(response);
        }

        [Fact]
        public async Task CategoryId_DeleteCategory_CategoryIsDeleted()
        {
            const int categoryId = 4;
            var existingCategory = new Category
            {
                CategoryId = 4,
                CategoryName = "Delete Category"
            };
            _categoryService.Setup(s => s.GetById(categoryId)).ReturnsAsync(existingCategory);
            _categoryService.Setup(s => s.IsSavedToDb()).ReturnsAsync(true);

            var response = await _categoriesController.DeleteCategory(categoryId);

            Assert.IsType<OkObjectResult>(response);
        }

        [Fact]
        public async Task IdOfNonexistentCategory_GetCategoryProducts_ReturnNotFound()
        {
            const int categoryId = -1;

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _categoriesController.GetCategoryProducts(categoryId));

            Assert.Equal(StatusCodes.Status404NotFound, exception.Details.Status);
            Assert.Contains("category", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task IdOfNonexistentCategory_GetCategoryProduct_ReturnNotFound()
        {
            const int categoryId = -1;
            const int productId = 7;

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _categoriesController.GetCategoryProduct(categoryId, productId));

            Assert.Equal(StatusCodes.Status404NotFound, exception.Details.Status);
            Assert.Contains("category", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task IdOfNonexistentProduct_GetCategoryProduct_ReturnNotFound()
        {
            const int categoryId = 3;
            const int productId = -1;

            _categoryService.Setup(c => c.GetById(categoryId)).ReturnsAsync(new Category());

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _categoriesController.GetCategoryProduct(categoryId, productId));

            Assert.Equal(StatusCodes.Status404NotFound, exception.Details.Status);
            Assert.Contains("product", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task IdOfNonexistentCategory_AddCategoryProduct_ReturnNotFound()
        {
            const int categoryId = -1;
            var productModel = new ProductModel();

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _categoriesController.AddCategoryProduct(categoryId, productModel));

            Assert.Equal(StatusCodes.Status404NotFound, exception.Details.Status);
            Assert.Contains("category", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task FailedToSaveNewProduct_AddCategoryProduct_ReturnBadRequest()
        {
            const int categoryId = -1;
            var productModel = new ProductModel();

            _categoryService.Setup(c => c.GetById(categoryId)).ReturnsAsync(new Category());

            var response = await _categoriesController.AddCategoryProduct(categoryId, productModel);

            Assert.IsType<ActionResult<ProductModel>>(response);
            Assert.IsType<BadRequestResult>(response.Result);
            _categoryService.Verify(c => c.AddEntity(categoryId, It.IsAny<Product>()));
        }

        [Fact]
        public async Task ProductModel_AddCategoryProduct_ResponseContainsLocationInHeader()
        {
            const int categoryId = 4;
            var productModel = new ProductModel();

            _categoryService.Setup(s => s.GetById(categoryId)).ReturnsAsync(new Category());
            _categoryService.Setup(s => s.IsSavedToDb()).ReturnsAsync(true);

            var response = await _categoriesController.AddCategoryProduct(categoryId, productModel);

            Assert.IsType<ActionResult<ProductModel>>(response);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(response.Result);

            Assert.IsType<ProductModel>(createdAtActionResult.Value);
            Assert.Equal(StatusCodes.Status201Created, createdAtActionResult.StatusCode);
            Assert.Equal(2, createdAtActionResult.RouteValues.Keys.Count);
            Assert.Contains("categoryId", createdAtActionResult.RouteValues.Keys);
            Assert.Contains("productId", createdAtActionResult.RouteValues.Keys);
        }

        [Fact]
        public async Task IdOfNonexistentCategory_UpdateCategoryProduct_ReturnNotFound()
        {
            const int categoryId = -1;
            const int productId = 6;
            var productModel = new ProductModel();

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _categoriesController.UpdateCategoryProduct(categoryId, productId, productModel));

            Assert.Equal(StatusCodes.Status404NotFound, exception.Details.Status);
            Assert.Contains("category", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task IdOfNonexistentProduct_UpdateCategoryProduct_ReturnNotFound()
        {
            const int categoryId = 3;
            const int productId = -1;
            var productModel = new ProductModel();

            _categoryService.Setup(c => c.GetById(categoryId)).ReturnsAsync(new Category());

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _categoriesController.UpdateCategoryProduct(categoryId, productId, productModel));

            Assert.Equal(StatusCodes.Status404NotFound, exception.Details.Status);
            Assert.Contains("product", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task FailedToSaveUpdatedProduct_UpdateCategoryProduct_ReturnBadRequest()
        {
            const int categoryId = 9;
            const int productId = 13;
            var productModel = new ProductModel();

            _categoryService.Setup(c => c.GetById(categoryId)).ReturnsAsync(new Category());
            _categoryService.Setup(s => s.GetEntityById(categoryId, productId))
                            .ReturnsAsync(new Product());

            var response = await _categoriesController.UpdateCategoryProduct(categoryId,
                                                                             productId,
                                                                             productModel);

            Assert.IsType<ActionResult<ProductModel>>(response);
            Assert.IsType<BadRequestResult>(response.Result);
            _categoryService.Verify(c => c.UpdateEntity(categoryId, It.IsAny<Product>()));
        }

        [Fact]
        public async Task IdOfNonexistentCategory_DeleteCategoryProduct_ReturnNotFound()
        {
            const int categoryId = -1;
            const int productId = 10;

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _categoriesController.DeleteCategoryProduct(categoryId, productId));

            Assert.Equal(StatusCodes.Status404NotFound, exception.Details.Status);
            Assert.Contains("category", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task IdOfNonexistentProduct_DeleteCategoryProduct_ReturnNotFound()
        {
            const int categoryId = 3;
            const int productId = -1;

            _categoryService.Setup(c => c.GetById(categoryId)).ReturnsAsync(new Category());

            var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() =>
                _categoriesController.DeleteCategoryProduct(categoryId, productId));

            Assert.Equal(StatusCodes.Status404NotFound, exception.Details.Status);
            Assert.Contains("product", exception.Details.Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task FailedToSaveChangesAfterDelete_DeleteCategoryProduct_ReturnBadRequest()
        {
            const int categoryId = 8;
            const int productId = 2;

            _categoryService.Setup(c => c.GetById(categoryId)).ReturnsAsync(new Category());
            _categoryService.Setup(s => s.GetEntityById(categoryId, productId))
                            .ReturnsAsync(new Product());

            var response = await _categoriesController.DeleteCategoryProduct(categoryId, productId);

            Assert.IsType<BadRequestResult>(response);
            _categoryService.Verify(c => c.DeleteEntity(categoryId, It.IsAny<Product>()));
        }
    }
}