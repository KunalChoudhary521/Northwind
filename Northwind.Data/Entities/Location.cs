namespace Northwind.Data.Entities
{
    public class Location
    {
        public int LocationId { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string Extension { get; set; }
        public string Fax { get; set; }

        // Navigation properties
        public Supplier Supplier { get; set; }
    }
}