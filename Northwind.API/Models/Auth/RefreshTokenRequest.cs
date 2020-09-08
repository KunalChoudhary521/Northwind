using System.ComponentModel.DataAnnotations;

namespace Northwind.API.Models.Auth
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}