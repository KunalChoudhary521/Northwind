using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Northwind.API.Models.Orders;

namespace Northwind.API.Models
{
    public class ShipperModel
    {
        public int ShipperId { get; set; }
        [Required]
        public string CompanyName { get; set; }
        public string Phone { get; set; }
        public ICollection<OrderResponseModel> Orders { get; set; }

        public ShipperModel()
        {
            Orders = new List<OrderResponseModel>();
        }
    }
}