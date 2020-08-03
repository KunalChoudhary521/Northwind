using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Northwind.Data.Entities
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string CompanyCode { get; set; }
        [Required]
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string ContactTitle { get; set; }
        public int LocationId { get; set; }

        // Navigation property
        public ICollection<Order> Orders { get; set; }
        public Location Location { get; set; }

        public Customer()
        {
            Orders = new List<Order>();
        }
    }
}