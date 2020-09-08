using System.ComponentModel.DataAnnotations;

namespace Northwind.API.Models.Auth
{
    public class AuthRequestModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}