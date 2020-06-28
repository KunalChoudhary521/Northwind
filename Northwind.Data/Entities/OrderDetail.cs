namespace Northwind.Data.Entities
{
    public class OrderDetail
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Discount { get; set; }

        // Navigation properties
        public Order Order { get; set; }
        public Product Product { get; set; }
    }
}