using System.ComponentModel.DataAnnotations;

namespace Northwind.API.Models
{
    public class CategoryModel
    {
        public int CategoryId { get; set; }
        [Required(ErrorMessage = "Category Name is required")]
        public string CategoryName { get; set; }
        public string Description { get; set; }
    }
}