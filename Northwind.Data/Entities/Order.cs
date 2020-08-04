using System;
using System.Collections.Generic;

namespace Northwind.Data.Entities
{
    public class Order
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public int? EmployeeId { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public DateTimeOffset RequiredDate { get; set; }
        public DateTimeOffset? ShippedDate { get; set; }
        public int? ShipperId { get; set; }
        public decimal Total { get; set; }
        public string ShipName { get; set; }
        public int LocationId { get; set; }

        // Navigation Properties
        public Customer Customer { get; set; }
        public Employee Employee { get; set; }
        public Shipper Shipper { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
        public Location Location { get; set; }

        public Order()
        {
            OrderDetails = new List<OrderDetail>();
        }
    }
}