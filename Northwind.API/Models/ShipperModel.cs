using System.ComponentModel.DataAnnotations;

namespace Northwind.API.Models
{
    public class ShipperModel
    {
        public int ShipperId { get; set; }
        [Required]
        public string CompanyName { get; set; }
        public string Phone { get; set; }
    }
}