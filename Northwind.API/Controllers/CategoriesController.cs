using System.Threading.Tasks;
using AutoMapper;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Northwind.API.Models;
using Northwind.API.Services;
using Northwind.Data.Entities;

namespace Northwind.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IMapper _mapper;

        public CategoriesController(ICategoryService categoryService, IMapper mapper)
        {
            _categoryService = categoryService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<CategoryModel[]>> GetCategories()
        {
            var categories = await _categoryService.GetAll();

            if (categories == null)
                return NotFound();

            var categoryModels = _mapper.Map<CategoryModel[]>(categories);

            return categoryModels;
        }

        [HttpGet("{categoryId:int}")]
        public async Task<ActionResult<CategoryModel>> GetCategory(int categoryId)
        {
            var category = await _categoryService.GetById(categoryId);

            if (category == null)
                return NotFound();

            var categoryModel = _mapper.Map<CategoryModel>(category);
            return categoryModel;
        }

        [HttpPost]
        public async Task<ActionResult<CategoryModel>> AddCategory(CategoryModel categoryModel)
        {
            if (await _categoryService.GetCategoryByName(categoryModel.CategoryName) != null)
                return BadRequest($"'{categoryModel.CategoryName}' already exists");

            var category = _mapper.Map<Category>(categoryModel);
            _categoryService.Add(category);
            if (await _categoryService.IsSavedToDb())
            {
                var persistedCategory = await _categoryService.GetCategoryByName(categoryModel.CategoryName);
                var persistedCategoryModel = _mapper.Map<CategoryModel>(persistedCategory);

                return CreatedAtAction("GetCategory", new {categoryId = persistedCategoryModel.CategoryId},
                                       persistedCategoryModel);
            }

            return BadRequest();
        }

        [HttpPut("{categoryId:int}")]
        public async Task<ActionResult<CategoryModel>> UpdateCategory(int categoryId, CategoryModel categoryModel)
        {
            var oldCategory = await _categoryService.GetById(categoryId);
            if (oldCategory == null)
                return NotFound();

            var updatedCategory = _mapper.Map(categoryModel, oldCategory);
            _categoryService.Update(updatedCategory);

            if (await _categoryService.IsSavedToDb())
                return Ok(_mapper.Map<CategoryModel>(updatedCategory));

            return BadRequest();
        }

        [HttpDelete("{categoryId:int}")]
        public async Task<ActionResult> DeleteCategory(int categoryId)
        {
            var existingCategory = await _categoryService.GetById(categoryId);
            if (existingCategory == null)
                return NotFound();

            await _categoryService.Delete(existingCategory);

            if (await _categoryService.IsSavedToDb())
                return Ok($"'{existingCategory.CategoryName}' category has been deleted");

            return BadRequest();
        }

        [HttpGet("{categoryId:int}/products")]
        public async Task<ActionResult<ProductModel[]>> GetCategoryProducts(int categoryId)
        {
            await DoesCategoryExist(categoryId);

            var products = await _categoryService.GetAllEntities(categoryId);
            return _mapper.Map<ProductModel[]>(products);
        }

        [HttpGet("{categoryId:int}/products/{productId:int}")]
        public async Task<ActionResult<ProductModel>> GetCategoryProduct(int categoryId, int productId)
        {
            await DoesCategoryExist(categoryId);

            var product = await GetProduct(categoryId, productId);

            return _mapper.Map<ProductModel>(product);
        }

        [HttpPost("{categoryId:int}/products")]
        public async Task<ActionResult<ProductModel>> AddCategoryProduct(int categoryId,
                                                                         ProductModel productModel)
        {
            await DoesCategoryExist(categoryId);

            var product = _mapper.Map<Product>(productModel);
            _categoryService.AddEntity(categoryId, product);
            if (await _categoryService.IsSavedToDb())
            {
                var persistedProductModel = _mapper.Map<ProductModel>(product);
                return CreatedAtAction(nameof(GetCategoryProduct),
                                       new { categoryId, productId = product.ProductId },
                                       persistedProductModel);
            }

            return BadRequest();
        }

        [HttpPut("{categoryId:int}/products/{productId:int}")]
        public async Task<ActionResult<ProductModel>> UpdateCategoryProduct(int categoryId,
                                                                            int productId,
                                                                            ProductModel productModel)
        {
            await DoesCategoryExist(categoryId);

            var oldProduct = await GetProduct(categoryId, productId);

            var newProduct = _mapper.Map(productModel, oldProduct);
            _categoryService.UpdateEntity(categoryId, newProduct);

            if (await _categoryService.IsSavedToDb())
                return Ok(_mapper.Map<ProductModel>(newProduct));

            return BadRequest();
        }

        [HttpDelete("{categoryId:int}/products/{productId:int}")]
        public async Task<ActionResult> DeleteCategoryProduct(int categoryId, int productId)
        {
            await DoesCategoryExist(categoryId);

            var existingProduct = await GetProduct(categoryId, productId);

            _categoryService.DeleteEntity(categoryId, existingProduct);

            if(await _categoryService.IsSavedToDb())
                return Ok($"Product with id '{productId}' of category " +
                          $"with id {categoryId} has been deleted");

            return BadRequest();
        }

        private async Task DoesCategoryExist(int categoryId)
        {
            if (await _categoryService.GetById(categoryId) == null)
                throw new ProblemDetailsException(StatusCodes.Status404NotFound,
                                                  $"Category with id {categoryId} not found");
        }

        private async Task<Product> GetProduct(int categoryId, int productId)
        {
            var product = await _categoryService.GetEntityById(categoryId, productId);

            if (product == null)
                throw new ProblemDetailsException(StatusCodes.Status404NotFound,
                                                  $"Product with id {productId} not found");
            return product;
        }
    }
}