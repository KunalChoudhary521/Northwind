using System.Collections.Generic;

namespace Northwind.Data.Entities
{
    public class Shipper
    {
        public int ShipperId { get; set; }
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