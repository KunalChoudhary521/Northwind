using System.ComponentModel.DataAnnotations;
using Northwind.API.Models.Auth;

namespace Northwind.API.Models.Users
{
    public class UserRequestModel : AuthRequestModel
    {
        [Required]
        public string Role { get; set; }
    }
}