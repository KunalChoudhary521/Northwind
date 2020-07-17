using AutoMapper;
using Northwind.API.Models;
using Northwind.API.Profiles;
using Northwind.Data.Entities;
using Xunit;

namespace Northwind.Tests.UnitTests.Profiles
{
    public class SupplierProfileTests
    {
        private readonly IMapper _mapper;

        public SupplierProfileTests()
        {
            var mapperConfig = new MapperConfiguration(config =>
            {
                config.AddProfiles(new Profile[] { new SupplierProfile(), new LocationProfile() });
            });
            _mapper = mapperConfig.CreateMapper();
        }

        [Fact]
        public void Supplier_SupplierToSupplierModel_ReturnSupplierModel()
        {
            var supplier = new Supplier
            {
                SupplierId = 5,
                CompanyName = "Tokyo Traders",
                ContactName = "Yoshi Nagase",
                ContactTitle = "Marketing Manager",
                HomePage = "tokyotraders.com",
                Location = new Location
                {
                    LocationId = 93,
                    Address = "9-8 Sekimai Musashino-shi",
                    City = "Tokyo",
                    Region = "Kanto",
                    PostalCode = "100-1002",
                    Country = "Japan",
                    Phone = "(03) 3555-5011",
                    Extension = "100",
                    Fax = "(03) 3555-5010"
                }
            };

            var supplierModel = _mapper.Map<SupplierModel>(supplier);
            Assert.Equal(5, supplierModel.SupplierId);
            Assert.Equal("Tokyo Traders", supplierModel.CompanyName);
            Assert.Equal("Yoshi Nagase", supplierModel.ContactName);
            Assert.Equal("Marketing Manager", supplierModel.ContactTitle);
            Assert.Equal("tokyotraders.com", supplierModel.HomePage);

            var locationModel = supplierModel.Location;
            Assert.Equal("9-8 Sekimai Musashino-shi", locationModel.Address);
            Assert.Equal("Tokyo", locationModel.City);
            Assert.Equal("Kanto", locationModel.Region);
            Assert.Equal("100-1002", locationModel.PostalCode);
            Assert.Equal("Japan", locationModel.Country);
            Assert.Equal("(03) 3555-5011", locationModel.Phone);
            Assert.Equal("100", locationModel.Extension);
            Assert.Equal("(03) 3555-5010", locationModel.Fax);
        }

        [Fact]
        public void SupplierModelWithSupplierId_SupplierModelToSupplier_ReturnSupplierWithIdZero()
        {
            var supplierModel = new SupplierModel
            {
                SupplierId = 30,
                CompanyName = "Ma Maison",
                Location = new LocationModel
                {
                    Address = "2960 Rue St. Laurent",
                    City = "Montreal",
                    Region = "Quebec",
                    PostalCode = "H1J 1C3",
                    Country = "Canada",
                    Phone = "(514) 555-9022"
                }
            };

            var supplier = _mapper.Map<Supplier>(supplierModel);

            Assert.Equal(0, supplier.SupplierId);
            Assert.Equal("Ma Maison", supplier.CompanyName);
            Assert.Null(supplier.ContactName);

            var location = supplier.Location;
            Assert.Equal("2960 Rue St. Laurent", location.Address);
            Assert.Equal("Montreal", location.City);
            Assert.Equal("Quebec", location.Region);
            Assert.Equal("H1J 1C3", location.PostalCode);
            Assert.Equal("Canada", location.Country);
            Assert.Equal("(514) 555-9022", location.Phone);
            Assert.Null(location.Extension);
        }
    }
}