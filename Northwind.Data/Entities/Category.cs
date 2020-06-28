using System.Collections.Generic;

namespace Northwind.Data.Entities
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }

        // Navigation property
        public ICollection<Product> Products { get; set; }

        public Category()
        {
            Products = new List<Product>();
        }
    }
}