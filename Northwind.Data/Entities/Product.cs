using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Northwind.Data.Entities
{
    public class Product
    {
        public int ProductId { get; set; }
        public int? CategoryId { get; set; }
        [Required]
        public string ProductName { get; set; }
        public int? SupplierId { get; set; }
        public string QuantityPerUnit { get; set; }
        public decimal UnitPrice { get; set; }
        public int UnitsInStock { get; set; }
        public int UnitsOnOrder { get; set; }
        public int ReorderLevel { get; set; }
        [Required]
        public int Discontinued { get; set; }

        // Navigation properties
        public Category Category { get; set; }
        public Supplier Supplier { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }

        public Product()
        {
            OrderDetails = new List<OrderDetail>();
        }
    }
}