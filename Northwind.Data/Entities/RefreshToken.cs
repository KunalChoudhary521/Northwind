using System;

namespace Northwind.Data.Entities
{
    public class RefreshToken
    {
        public int RefreshTokenId { get; set; }
        public string Value { get; set; }
        public DateTimeOffset? ExpiryDate { get; set; }
        public DateTimeOffset? CreateDate { get; set; }
        public DateTimeOffset? RevokeDate { get; set; }
    }
}