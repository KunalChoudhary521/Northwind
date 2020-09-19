using System;

namespace Northwind.Data.Entities
{
    public class User
    {
        public int UserId { get; set; }
        public Guid UserIdentifier { get; set; }
        public string UserName { get; set; }
        public byte[] PasswordSalt { get; set; }
        public byte[] PasswordHash { get; set; }
        public string AccessToken { get; set; }
        public Role Role { get; set; }

        // Navigation property
        public RefreshToken RefreshToken { get; set; }
    }
}