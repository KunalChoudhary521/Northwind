using System;
using Northwind.API.Models.Auth;

namespace Northwind.API.Models
{
    public class UserModel
    {
        public int UserId { get; set; }
        public Guid UserIdentifier { get; set; }
        public string UserName { get; set; }
        public string AccessToken { get; set; }
        public RefreshTokenModel RefreshToken { get; set; }
    }
}