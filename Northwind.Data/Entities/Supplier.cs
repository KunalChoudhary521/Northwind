using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Northwind.Data.Entities
{
    public class Supplier
    {
        public int SupplierId { get; set; }
        [Required]
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string ContactTitle { get; set; }
        public string HomePage { get; set; }
        public int LocationId { get; set; }

        // Navigation property
        public ICollection<Product> Products { get; set; }
        public Location Location { get; set; }

        public Supplier()
        {
            Products = new List<Product>();
        }
    }
}