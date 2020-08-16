using System;
using System.Collections.Generic;

namespace Northwind.API.Models.Orders
{
    public class OrderResponseModel : OrderModelBase
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public int? ShipperId { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public DateTimeOffset? ShippedDate { get; set; }
        public decimal Total { get; set; }
        public string ShipName { get; set; }
        public ICollection<OrderItemResponseModel> OrderItems { get; set; }

        public OrderResponseModel()
        {
            OrderItems = new List<OrderItemResponseModel>();
        }
    }
}