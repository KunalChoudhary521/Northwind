using AutoMapper;
using Northwind.API.Models;
using Northwind.API.Profiles;
using Northwind.Data.Entities;
using Xunit;

namespace Northwind.Tests.UnitTests.Profiles
{
    public class ShipperProfileTests
    {
        private readonly IMapper _mapper;

        public ShipperProfileTests()
        {
            var mapperConfig = new MapperConfiguration(config =>
            {
                config.AddProfiles(new Profile[] { new ShipperProfile() });
            });
            _mapper = mapperConfig.CreateMapper();
        }

        [Fact]
        public void Shipper_ShipperToShipperModel_ReturnShipperModel()
        {
            var shipper = new Shipper
            {
                ShipperId = 12,
                CompanyName = "Test shipper",
                Phone = "(03) 3555-5011"
            };

            var shipperModel = _mapper.Map<ShipperModel>(shipper);

            Assert.Equal(12, shipper.ShipperId);
            Assert.Equal("Test shipper", shipper.CompanyName);
            Assert.Equal("(03) 3555-5011", shipper.Phone);
        }

        [Fact]
        public void ShipperModelWithShipperId_ShipperModelToShipper_ReturnShipperWithIdZero()
        {
            var shipperModel = new ShipperModel
            {
                ShipperId = 34,
                CompanyName = "Test shipper model",
                Phone = "416-901-5430"
            };

            var shipper = _mapper.Map<Shipper>(shipperModel);

            Assert.Equal(0, shipper.ShipperId);
            Assert.Equal("Test shipper model", shipper.CompanyName);
            Assert.Equal("416-901-5430", shipper.Phone);
        }
    }
}