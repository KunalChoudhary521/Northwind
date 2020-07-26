using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Northwind.API.Models;
using Northwind.API.Models.Auth;
using Northwind.API.Models.Users;
using Northwind.Data.Contexts;
using Northwind.Data.Entities;
using Northwind.ITTests.Helpers;
using Xunit;

namespace Northwind.ITTests.E2ETests
{
    [Collection(TestConstants.ItTests)]
    public class SupplierControllerE2E
    {
        private const string BaseSuppliersPath = "/api/suppliers";
        private readonly NorthwindContext _dbContext;
        private readonly HttpClient _client;
        private readonly UserHelper _userHelper;
        private readonly Action<string> _setAuthHeader;
        private readonly Func<AuthenticationHeaderValue> _getAuthHeader;

        public SupplierControllerE2E(ITTestFixture fixture)
        {
            _client = fixture.Client;
            _dbContext = fixture.DbContext;
            _userHelper = fixture.UserHelper;
            _getAuthHeader = fixture.GetAuthenticationHeader;
            _setAuthHeader = fixture.SetAuthenticationHeader;
        }

        [Fact]
        public async Task SuppliersFromSeedData_GetSuppliers_SuppliersReturned()
        {
            var getResponse = await _client.GetAsync(BaseSuppliersPath);

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var suppliers = await getResponse.Content.ReadAsAsync<SupplierModel[]>();
            Assert.Equal(2, suppliers.Length);
        }

        [Fact]
        public async Task SupplierModelAndSupplierAdminUser_CallSupplierEndpoints_CreateUpdateDeleteSupplier()
        {
            var supplierModel = new SupplierModel
            {
                CompanyName = "E2E supplier",
                ContactName = "Mr. Dev",
                ContactTitle = "Developer",
                Location = new LocationModel
                {
                    Address = "100 Comet street",
                    City = "Planetville",
                    Region = "Cali",
                    Country = "USA",
                    PostalCode = "674571",
                    Phone = "(123) 456 7890"
                }
            };

            // Create Supplier admin user & credentials
            var userRequestModel = CreateSupplierUser("SupplierAdmin");
            var createResponse = await _userHelper.CreateUser(userRequestModel);
            var credResponse = await _userHelper.CreateCredentials(userRequestModel);
            var supplierUser = await credResponse.Content.ReadAsAsync<AuthResponseModel>();

            var initialToken = _getAuthHeader().Parameter;

            // CREATE
            _setAuthHeader(supplierUser.AccessToken);
            var postResponse = await _client.PostAsJsonAsync(BaseSuppliersPath, supplierModel);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
            Assert.NotNull(postResponse.Headers.Location);

            var supplierId = int.Parse(postResponse.Headers.Location.Segments.Last());

            // Retrieve newly created supplier
            var getResponse = await _client.GetAsync($"{BaseSuppliersPath}/{supplierId}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var supplierRetrieved = await getResponse.Content.ReadAsAsync<SupplierModel>();
            Assert.Equal("E2E supplier", supplierRetrieved.CompanyName);
            Assert.Equal("Mr. Dev", supplierRetrieved.ContactName);
            Assert.Equal("100 Comet street", supplierRetrieved.Location.Address);
            Assert.Null(supplierRetrieved.Location.Fax);

            // UPDATE
            supplierModel.CompanyName = "Update operation - E2E supplier";
            supplierModel.Location.Fax = "(987) 654 321";

            var putResponse = await _client.PutAsJsonAsync($"{BaseSuppliersPath}/{supplierId}", supplierModel);
            Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

            var supplierUpdated = await putResponse.Content.ReadAsAsync<SupplierModel>();
            Assert.Equal("Update operation - E2E supplier", supplierUpdated.CompanyName);
            Assert.Equal("(987) 654 321", supplierUpdated.Location.Fax);

            // DELETE
            var deleteResponse = await _client.DeleteAsync($"{BaseSuppliersPath}/{supplierId}");
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

            // Try to retrieve deleted supplier
            getResponse = await _client.GetAsync($"{BaseSuppliersPath}/{supplierId}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

            // Delete user (using admin token)
            _setAuthHeader(initialToken);
            var userId = int.Parse(createResponse.Headers.Location.Segments.Last());
            await _userHelper.DeleteUser(userId);
        }

        [Fact]
        public async Task SupplierProductsFromSeedData_GetSupplierProducts_ProductsReturned()
        {
            const int supplierId = 2;
            var getResponse = await _client.GetAsync($"{BaseSuppliersPath}/{supplierId}/products");

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var products = await getResponse.Content.ReadAsAsync<ProductModel[]>();
            Assert.Equal(2, products.Length);
        }

        [Fact]
        public async Task ProductModel_CallSupplierProductEndpoints_CreateUpdateDeleteProduct()
        {
            const int supplierId = 1;
            var supplierProductUri = $"{BaseSuppliersPath}/{supplierId}/products";

            var productModel = new ProductModel
            {
                ProductName = "Product from E2E test",
                UnitsInStock = 10,
                UnitPrice = new decimal(4.99)
            };

            // CREATE
            var postResponse = await _client.PostAsJsonAsync(supplierProductUri, productModel);

            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
            Assert.NotNull(postResponse.Headers.Location);

            int productId = int.Parse(postResponse.Headers.Location.Segments.Last());

            // Retrieve newly created product
            var getResponse = await _client.GetAsync($"{supplierProductUri}/{productId}");

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var productRetrieved = await getResponse.Content.ReadAsAsync<ProductModel>();
            Assert.Equal(supplierId, productRetrieved.SupplierId);
            Assert.Equal("Product from E2E test", productRetrieved.ProductName);
            Assert.Equal(new decimal(4.99), productRetrieved.UnitPrice);
            Assert.Null(productRetrieved.QuantityPerUnit);

            // UPDATE
            productModel.UnitPrice = new decimal(7.99);
            productModel.QuantityPerUnit = "5 boxes";

            var putResponse = await _client.PutAsJsonAsync($"{supplierProductUri}/{productId}", productModel);

            Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

            var productUpdated = await putResponse.Content.ReadAsAsync<ProductModel>();
            Assert.Equal(supplierId, productUpdated.SupplierId);
            Assert.Equal(new decimal(7.99), productUpdated.UnitPrice);
            Assert.Equal("5 boxes", productUpdated.QuantityPerUnit);

            // DELETE (detach product from supplier)
            var deleteResponse = await _client.DeleteAsync($"{supplierProductUri}/{productId}");
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

            // Try to retrieve deleted product
            getResponse = await _client.GetAsync($"{supplierProductUri}/{productId}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

            // Clean up product
            var productToRemove = await _dbContext.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
            _dbContext.Products.Remove(productToRemove);
            await _dbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task SupplierWithProducts_DeleteSupplier_ProductsDetachedFromSupplier()
        {
            var supplierModel = new SupplierModel
            {
                CompanyName = "E2E supplier with products",
                Location = new LocationModel
                {
                    Address = "1 Skylark Rd."
                }
            };

            // Add supplier
            var postSupplierResponse = await _client.PostAsJsonAsync(BaseSuppliersPath, supplierModel);
            int supplierId = int.Parse(postSupplierResponse.Headers.Location.Segments.Last());

            Assert.Equal(HttpStatusCode.Created, postSupplierResponse.StatusCode);
            Assert.NotNull(postSupplierResponse.Headers.Location);

            var productModel = new ProductModel
            {
                ProductName = "Product 1 with supplier",
                UnitsInStock = 5,
                UnitPrice = new decimal(2.99)
            };

            // Add product to supplier
            var supplierProductUri = $"{BaseSuppliersPath}/{supplierId}/products";
            var postProductResponse = await _client.PostAsJsonAsync(supplierProductUri, productModel);
            int productId = int.Parse(postProductResponse.Headers.Location.Segments.Last());

            Assert.Equal(HttpStatusCode.Created, postProductResponse.StatusCode);
            Assert.NotNull(postProductResponse.Headers.Location);

            //Delete supplier
            var deleteSupplierResponse = await _client.DeleteAsync($"{BaseSuppliersPath}/{supplierId}");
            Assert.Equal(HttpStatusCode.OK, deleteSupplierResponse.StatusCode);

            // Clean up product
            var products = _dbContext.Products;
            var productWithoutSupplier = await products.FirstOrDefaultAsync(p => p.ProductId == productId);
            Assert.Null(productWithoutSupplier.SupplierId);

            _dbContext.Products.Remove(productWithoutSupplier);
            await _dbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task SupplierUser_CreateUpdateDeleteSupplier_ReturnForbidden()
        {
            var userRequestModel = CreateSupplierUser("Supplier");

            // Create Supplier user & credentials
            var createResponse = await _userHelper.CreateUser(userRequestModel);
            var credResponse = await _userHelper.CreateCredentials(userRequestModel);
            var supplierUser = await credResponse.Content.ReadAsAsync<AuthResponseModel>();

            var initialToken = _getAuthHeader().Parameter;

            // Create supplier
            var supplierModel = new SupplierModel { CompanyName = "E2E supplier" };

            _setAuthHeader(supplierUser.AccessToken);
            var postResponse = await _client.PostAsJsonAsync(BaseSuppliersPath, supplierModel);
            Assert.Equal(HttpStatusCode.Forbidden, postResponse.StatusCode);

            // Update supplier
            var putResponse = await _client.PutAsJsonAsync($"{BaseSuppliersPath}/{int.MaxValue}", supplierModel);
            Assert.Equal(HttpStatusCode.Forbidden, putResponse.StatusCode);

            // Delete supplier
            var deleteResponse = await _client.DeleteAsync($"{BaseSuppliersPath}/{int.MaxValue}");
            Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);

            // Delete user (using admin token)
            _setAuthHeader(initialToken);
            var userId = int.Parse(createResponse.Headers.Location.Segments.Last());
            await _userHelper.DeleteUser(userId);
        }

        [Fact]
        public async Task SupplierUser_RetrieveSeedSuppliers_ReturnOk()
        {
            var userRequestModel = CreateSupplierUser("Supplier");

            // Create Supplier user & credentials
            var createResponse = await _userHelper.CreateUser(userRequestModel);
            var credResponse = await _userHelper.CreateCredentials(userRequestModel);
            var supplierUser = await credResponse.Content.ReadAsAsync<AuthResponseModel>();

            var initialToken = _getAuthHeader().Parameter;

            // Get All suppliers
            _setAuthHeader(supplierUser.AccessToken);
            var getResponse = await _client.GetAsync($"{BaseSuppliersPath}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            // Get supplier by id
            const int supplierId = 1;
            getResponse = await _client.GetAsync($"{BaseSuppliersPath}/{supplierId}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            // Delete user (using admin token)
            _setAuthHeader(initialToken);
            var userId = int.Parse(createResponse.Headers.Location.Segments.Last());
            await _userHelper.DeleteUser(userId);
        }

        private static UserRequestModel CreateSupplierUser(string role)
        {
            return new UserRequestModel
            {
                UserName = "testSupplier",
                Password = "testSupplier",
                Role = role
            };
        }
    }
}