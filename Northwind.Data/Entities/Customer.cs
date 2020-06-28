using System.Collections.Generic;

namespace Northwind.Data.Entities
{
    public class Customer
    {
        public string CustomerId { get; set; }
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string ContactTitle { get; set; }

        // Navigation property
        public ICollection<Order> Orders { get; set; }

        // TODO: Refactor: Move address fields in a new class
        public string Address { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }

        public Customer()
        {
            Orders = new List<Order>();
        }
    }
}