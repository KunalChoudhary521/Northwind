using System;
using System.ComponentModel.DataAnnotations;

namespace Northwind.API.Models.Orders
{
    public class OrderModelBase
    {
        [Required]
        public DateTimeOffset RequiredDate { get; set; }
    }
}