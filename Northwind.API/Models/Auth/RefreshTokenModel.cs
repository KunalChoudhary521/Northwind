using System;

namespace Northwind.API.Models.Auth
{
    public class RefreshTokenModel
    {
        public string Value { get; set; }
        public DateTimeOffset? ExpiryDate { get; set; }
        public DateTimeOffset? CreateDate { get; set; }
        public DateTimeOffset? RevokeDate { get; set; }
        public bool IsRevoked => RevokeDate != null && ExpiryDate == null;
    }
}