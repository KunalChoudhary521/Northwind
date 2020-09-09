namespace Northwind.API.Helpers
{
    public class AuthSettings
    {
        public string SecretKey { get; set; }
        public long ExpiryDuration { get; set; }
        public string ValidAudience { get; set; }
        public string ValidIssuer { get; set; }
    }
}