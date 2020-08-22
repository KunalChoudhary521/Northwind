using System;
using System.ComponentModel.DataAnnotations;

namespace Northwind.API.Models
{
    public class ShipperOrderModel
    {
        [Required]
        public DateTimeOffset ShippedDate { get; set; }
        public string ShipName { get; set; }
    }
}