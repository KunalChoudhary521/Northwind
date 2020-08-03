using System.ComponentModel.DataAnnotations;

namespace Northwind.API.Models
{
    public class CustomerModel
    {
        public int CustomerId { get; set; }
        public string CompanyCode { get; set; }
        [Required(ErrorMessage = "Company Name is required")]
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string ContactTitle { get; set; }
        public LocationModel Location { get; set; }
    }
}