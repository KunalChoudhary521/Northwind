using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Northwind.Data.Entities
{
    public class Shipper
    {
        public int ShipperId { get; set; }
        [Required]
        public string CompanyName { get; set; }
        public string Phone { get; set; }

        // Navigation property
        public ICollection<Order> Orders { get; set; }

        public Shipper()
        {
            Orders = new List<Order>();
        }
    }
}