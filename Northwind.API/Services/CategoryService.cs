using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Northwind.API.Repositories;
using Northwind.Data.Entities;

namespace Northwind.API.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(IRepository<Category> categoryRepository,
                               IRepository<Product> productRepository,
                               ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<ICollection<Category>> GetAll()
        {
            _logger.LogInformation("Retrieving all categories");
            return await _categoryRepository.FindAll().ToArrayAsync();
        }

        public async Task<Category> GetById(int categoryId)
        {
            _logger.LogInformation($"Retrieving category with id: {categoryId}");
            return await _categoryRepository.FindByCondition(cat => cat.CategoryId == categoryId)
                                            .FirstOrDefaultAsync();
        }

        public async Task<Category> GetCategoryByName(string categoryName)
        {
            _logger.LogInformation($"Retrieving category with name: {categoryName}");
            return await _categoryRepository.FindByCondition(cat => cat.CategoryName == categoryName)
                                            .FirstOrDefaultAsync();
        }

        public void Add(Category category)
        {
            _logger.LogInformation($"Adding a new category: {category.CategoryName}");
            _categoryRepository.Add(category);
        }

        public void Update(Category category)
        {
            _logger.LogInformation($"Updating an existing category: {category.CategoryName}");
            _categoryRepository.Update(category);
        }

        public async Task Delete(Category category)
        {
            _logger.LogInformation($"Detach products from category: {category.CategoryName}");

            await _productRepository.FindByCondition(p => p.CategoryId == category.CategoryId)
                                    .ForEachAsync(p => p.CategoryId = null);

            await _productRepository.SaveChangesAsync();

            _logger.LogInformation($"Deleting a category: {category.CategoryName}");
            _categoryRepository.Delete(category);
        }

        public async Task<bool> IsSavedToDb()
        {
            return await _categoryRepository.SaveChangesAsync();
        }

        public async Task<ICollection<Product>> GetAllEntities(int categoryId)
        {
            _logger.LogInformation($"Retrieving products of category with id: {categoryId}");
            return await QueryProductsById(categoryId).ToArrayAsync();
        }

        public async Task<Product> GetEntityById(int categoryId, int productId)
        {
            _logger.LogInformation($"Retrieving product with id {productId} " +
                                   $"of category with id: {categoryId}");
            return await QueryProductsById(categoryId)
                            .FirstOrDefaultAsync(product => product.ProductId == productId);
        }

        public void AddEntity(int categoryId, Product product)
        {
            _logger.LogInformation($"Adding product {product.ProductName}" +
                                   $" to category with id: {categoryId}");

            // Attach product to category
            product.CategoryId = categoryId;
            _productRepository.Add(product);
        }

        public void UpdateEntity(int categoryId, Product product)
        {
            _logger.LogInformation($"Updating product {product.ProductName}" +
                                   $" to category with id: {categoryId}");

            // Attach product to category
            product.CategoryId = categoryId;
            _productRepository.Update(product);
        }

        public void DeleteEntity(int categoryId, Product product)
        {
            _logger.LogInformation($"Deleting product with id ${product.ProductId}" +
                                   $" from category with id: {categoryId}");

            // Detach product from category, but retain the product in DB
            product.CategoryId = null;
        }

        private IQueryable<Product> QueryProductsById(int categoryId)
        {
            return _productRepository.FindByCondition(product => product.CategoryId == categoryId);
        }
    }
}