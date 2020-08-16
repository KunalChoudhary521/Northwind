namespace Northwind.API.Models.Orders
{
    public class OrderItemResponseModel : OrderItemBase
    {
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
    }
}