using System;

namespace Northwind.API.Models.Auth
{
    public class AuthResponseModel
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTimeOffset RefreshTokenExpiryDate { get; set; }
    }
}