using System.Threading.Tasks;
using AutoMapper;
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
            var categories = await _categoryService.GetAllCategories();

            if (categories == null)
                return NotFound();

            var categoryModels = _mapper.Map<CategoryModel[]>(categories);

            return categoryModels;
        }

        [HttpGet("{categoryId:int}")]
        public async Task<ActionResult<CategoryModel>> GetCategory(int categoryId)
        {
            var category = await _categoryService.GetCategoryById(categoryId);

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
            _categoryService.AddCategory(category);
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
            var oldCategory = await _categoryService.GetCategoryById(categoryId);
            if (oldCategory == null)
                return NotFound();

            var updatedCategory = _mapper.Map(categoryModel, oldCategory);
            _categoryService.UpdateCategory(updatedCategory);

            if (await _categoryService.IsSavedToDb())
                return Ok(_mapper.Map<CategoryModel>(updatedCategory));

            return BadRequest();
        }

        [HttpDelete("{categoryId:int}")]
        public async Task<ActionResult> DeleteCategory(int categoryId)
        {
            var existingCategory = await _categoryService.GetCategoryById(categoryId);
            if (existingCategory == null)
                return NotFound();

            _categoryService.DeleteCategory(existingCategory);

            if (await _categoryService.IsSavedToDb())
                return Ok($"'{existingCategory.CategoryName}' category has been deleted");

            return BadRequest();
        }
    }
}