using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Northwind.API.Repositories;
using Northwind.Data.Entities;

namespace Northwind.API.Services
{
    public class CategoryService
    {
        private readonly IRepository<Category> _categoryRepository;
        private readonly ProductRepository _productRepository;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(IRepository<Category> categoryRepository,
                               ProductRepository productRepository,
                               ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<Category[]> GetAllCategories()
        {
            _logger.LogInformation("Retrieving all categories");
            return await _categoryRepository.FindAll().ToArrayAsync();
        }

        public async Task<Category> GetCategoryById(int categoryId)
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

        public void AddCategory(Category category)
        {
            _logger.LogInformation($"Adding a new category: {category.CategoryName}");
            _categoryRepository.Add(category);
        }

        public void UpdateCategory(Category category)
        {
            _logger.LogInformation($"Updating an existing category: {category.CategoryName}");
            _categoryRepository.Update(category);
        }

        public async void DeleteCategory(Category category)
        {
            _logger.LogInformation($"Detach products from category: {category.CategoryName}");
            var products = await _productRepository.FindByCategoryId(category.CategoryId).ToArrayAsync();

            foreach (var product in products)
            {
                product.CategoryId = null;
            }

            _logger.LogInformation($"Deleting a category: {category.CategoryName}");
            _categoryRepository.Delete(category);
        }

        public async Task<bool> IsSavedToDb()
        {
            return await _categoryRepository.SaveChangesAsync();
        }
    }
}