using System.Collections.Generic;

namespace Northwind.API.Models.Orders
{
    public class OrderRequestModel : OrderModelBase
    {
        public ICollection<OrderItemRequestModel> OrderItems { get; set; }

        public OrderRequestModel()
        {
            OrderItems = new List<OrderItemRequestModel>();
        }
    }
}