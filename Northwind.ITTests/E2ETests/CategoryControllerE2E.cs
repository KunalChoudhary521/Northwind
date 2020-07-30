using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Northwind.API.Models;
using Northwind.Data.Contexts;
using Xunit;

namespace Northwind.ITTests.E2ETests
{
    [Collection(TestConstants.ItTests)]
    public class CategoryControllerE2E
    {
        private const string BaseCategoryPath = "/api/categories";
        private readonly NorthwindContext _dbContext;
        private readonly HttpClient _client;

        public CategoryControllerE2E(ITTestFixture fixture)
        {
            _client = fixture.Client;
            _dbContext = fixture.DbContext;
        }

        [Fact]
        public async Task CategoriesFromSeedData_GetCategories_CategoriesReturned()
        {
            var getResponse = await _client.GetAsync(BaseCategoryPath);

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var categories = await getResponse.Content.ReadAsAsync<CategoryModel[]>();
            Assert.Equal(2, categories.Length);
        }

        [Fact]
        public async Task CategoryModel_CallCategoryEndpoints_CreateUpdateDeleteCategory()
        {
            CategoryModel categoryModel = new CategoryModel
            {
                CategoryName = "E2E category"
            };

            // CREATE
            var postResponse = await _client.PostAsJsonAsync(BaseCategoryPath, categoryModel);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
            Assert.NotNull(postResponse.Headers.Location);

            int categoryId = int.Parse(postResponse.Headers.Location.Segments.Last());

            // Retrieve newly created category
            var getResponse = await _client.GetAsync($"{BaseCategoryPath}/{categoryId}");

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var categoryRetrieved = await getResponse.Content.ReadAsAsync<CategoryModel>();
            Assert.Equal("E2E category", categoryRetrieved.CategoryName);
            Assert.Null(categoryRetrieved.Description);

            // UPDATE
            categoryModel.Description = "Update operation - E2E category";

            var putResponse = await _client.PutAsJsonAsync($"{BaseCategoryPath}/{categoryId}", categoryModel);

            Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

            var categoryUpdated = await putResponse.Content.ReadAsAsync<CategoryModel>();
            Assert.Equal(categoryId, categoryUpdated.CategoryId);
            Assert.Equal("Update operation - E2E category", categoryModel.Description);

            // DELETE
            var deleteResponse = await _client.DeleteAsync($"{BaseCategoryPath}/{categoryId}");
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

            // Try to retrieve deleted category
            getResponse = await _client.GetAsync($"{BaseCategoryPath}/{categoryId}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task CategoryFromSeedData_GetCategoryProducts_ProductsReturned()
        {
            const int categoryId = 2;
            var getResponse = await _client.GetAsync($"{BaseCategoryPath}/{categoryId}/products");

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var products = await getResponse.Content.ReadAsAsync<ProductModel[]>();
            Assert.Equal(2, products.Length);
        }

        [Fact]
        public async Task ProductModel_CallCategoryProductEndpoints_CreateUpdateDeleteProduct()
        {
            const int categoryId = 1;
            var categoryProductUrl = $"{BaseCategoryPath}/{categoryId}/products";

            var productModel = new ProductModel
            {
                ProductName = "Category Product - E2E test",
                UnitsInStock = 3,
                UnitPrice = new decimal(0.99),
                SupplierId = 1
            };

            // CREATE
            var postResponse = await _client.PostAsJsonAsync(categoryProductUrl, productModel);

            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
            Assert.NotNull(postResponse.Headers.Location);

            int productId = int.Parse(postResponse.Headers.Location.Segments.Last());

            // Retrieve newly created product
            var getResponse = await _client.GetAsync($"{categoryProductUrl}/{productId}");

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var productRetrieved = await getResponse.Content.ReadAsAsync<ProductModel>();
            Assert.Equal(categoryId, productRetrieved.CategoryId);
            Assert.Equal("Category Product - E2E test", productRetrieved.ProductName);
            Assert.Equal(new decimal(0.99), productRetrieved.UnitPrice);
            Assert.Equal(1, productRetrieved.SupplierId);
            Assert.Null(productRetrieved.QuantityPerUnit);

            // UPDATE
            productModel.UnitPrice = new decimal(5.49);
            productModel.QuantityPerUnit = "2 kg";

            var putResponse = await _client.PutAsJsonAsync($"{categoryProductUrl}/{productId}", productModel);

            Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

            var productUpdated = await putResponse.Content.ReadAsAsync<ProductModel>();
            Assert.Equal(categoryId, productUpdated.CategoryId);
            Assert.Equal(new decimal(5.49), productUpdated.UnitPrice);
            Assert.Equal("2 kg", productUpdated.QuantityPerUnit);

            // DELETE (detach product from category)
            var deleteResponse = await _client.DeleteAsync($"{categoryProductUrl}/{productId}");
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

            // Clean up product
            var productToRemove = await _dbContext.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
            _dbContext.Products.Remove(productToRemove);
            await _dbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task CategoryWithProducts_DeleteCategory_ProductsDetachedFromCategory()
        {
            var categoryModel = new CategoryModel
            {
                CategoryName = "E2E category with products"
            };

            // Add category
            var postCategoryResponse = await _client.PostAsJsonAsync(BaseCategoryPath, categoryModel);
            int categoryId = int.Parse(postCategoryResponse.Headers.Location.Segments.Last());

            Assert.Equal(HttpStatusCode.Created, postCategoryResponse.StatusCode);
            Assert.NotNull(postCategoryResponse.Headers.Location);

            var productModel = new ProductModel
            {
                ProductName = "Product 1 with category",
                UnitsInStock = 7,
                UnitPrice = new decimal(7.39),
                SupplierId = 2
            };

            // Add product to category
            var categoryProductUrl =  $"{BaseCategoryPath}/{categoryId}/products";
            var postProductResponse = await _client.PostAsJsonAsync(categoryProductUrl, productModel);
            int productId = int.Parse(postProductResponse.Headers.Location.Segments.Last());

            Assert.Equal(HttpStatusCode.Created, postProductResponse.StatusCode);
            Assert.NotNull(postProductResponse.Headers.Location);

            // Delete category
            var deleteCategoryResponse = await _client.DeleteAsync($"{BaseCategoryPath}/{categoryId}");
            Assert.Equal(HttpStatusCode.OK, deleteCategoryResponse.StatusCode);

            // Clean up product
            var productWithoutCategory =
                await _dbContext.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
            Assert.Null(productWithoutCategory.CategoryId);

            _dbContext.Products.Remove(productWithoutCategory);
            await _dbContext.SaveChangesAsync();
        }

    }
}