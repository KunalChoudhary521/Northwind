using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Northwind.API.Models;
using Northwind.API.Services;
using Northwind.Data.Entities;

namespace Northwind.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly CategoryService _categoryService;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public CategoriesController(CategoryService categoryService, IMapper mapper, LinkGenerator linkGenerator)
        {
            _categoryService = categoryService;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<CategoryModel[]>> GetCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllCategories();
                var categoryModels = _mapper.Map<CategoryModel[]>(categories);

                return categoryModels;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error: Unable to get all categories");
            }
        }

        [HttpGet("{categoryId:int}")]
        public async Task<ActionResult<CategoryModel>> GetCategory(int categoryId)
        {
            try
            {
                var category = await _categoryService.GetCategoryById(categoryId);

                if (category == null)
                    return NotFound();

                var categoryModel = _mapper.Map<CategoryModel>(category);
                return categoryModel;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  $"Error: Unable to get category with id: {categoryId}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<CategoryModel>> AddCategory(CategoryModel categoryModel)
        {
            try
            {
                var category = _mapper.Map<Category>(categoryModel);
                _categoryService.AddCategory(category);
                if (await _categoryService.IsSavedToDb())
                {
                    var categoryPersisted = await _categoryService.GetCategoryByName(categoryModel.CategoryName);
                    var location = _linkGenerator.GetPathByAction("GetCategory", "Categories",
                                                                  new {categoryId = categoryPersisted.CategoryId});

                    return Created(location, categoryPersisted);
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  $"Error: Unable to add category: {categoryModel.CategoryName}");
            }

            return BadRequest();
        }

        [HttpPut("{categoryId:int}")]
        public async Task<ActionResult<CategoryModel>> UpdateCategory(int categoryId, CategoryModel categoryModel)
        {
            try
            {
                var oldCategory = await _categoryService.GetCategoryById(categoryId);
                if (oldCategory == null)
                    return NotFound($"Unable to category with id: {categoryId}");

                var updatedCategory = _mapper.Map(categoryModel, oldCategory);
                _categoryService.UpdateCategory(updatedCategory);

                if (await _categoryService.IsSavedToDb())
                    return Ok(_mapper.Map<CategoryModel>(updatedCategory));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  $"Error: Unable to update category: {categoryModel.CategoryName}");
            }

            return BadRequest();
        }

        [HttpDelete("{categoryId:int}")]
        public async Task<ActionResult> DeleteCategory(int categoryId)
        {
            try
            {
                var existingCategory = await _categoryService.GetCategoryById(categoryId);
                if (existingCategory == null)
                    return NotFound($"Unable to find category with id: {categoryId}");

                _categoryService.DeleteCategory(existingCategory);

                if (await _categoryService.IsSavedToDb())
                    return Ok($"{existingCategory.CategoryName} category has been deleted");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  $"Error: Unable to delete category: {categoryId}");
            }

            return BadRequest();
        }
    }
}