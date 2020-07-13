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

        // Navigation property
        public ICollection<Product> Products { get; set; }

        // TODO: Refactor: Move address fields in a new class
        public string Address { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }

        public Supplier()
        {
            Products = new List<Product>();
        }
    }
}