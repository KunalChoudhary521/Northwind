using AutoMapper;
using Northwind.API.Models;
using Northwind.API.Profiles;
using Northwind.Data.Entities;
using Xunit;

namespace Northwind.Tests.UnitTests.Profiles
{
    public class CustomerProfileTests
    {
        private readonly IMapper _mapper;

        public CustomerProfileTests()
        {
            var mapperConfig = new MapperConfiguration(config =>
            {
                config.AddProfiles(new Profile[] { new CustomerProfile(), new LocationProfile() });
            });
            _mapper = mapperConfig.CreateMapper();
        }

        [Fact]
        public void Customer_CustomerToCustomerModel_ReturnCustomerModel()
        {
            var customer = new Customer
            {
                CustomerId = 15,
                CompanyName = "AROUT",
                CompanyCode = "Around the Horn",
                ContactName = "Thomas Hardy",
                ContactTitle = "Sales Representative",
                Location = new Location
                {
                    Address = "2732 Baker Blvd."
                }
            };

            var customerModel = _mapper.Map<CustomerModel>(customer);
            Assert.Equal(15, customerModel.CustomerId);
            Assert.Equal("AROUT", customerModel.CompanyName);
            Assert.Equal("Around the Horn", customerModel.CompanyCode);
            Assert.Equal("Thomas Hardy", customerModel.ContactName);
            Assert.Equal("Sales Representative", customerModel.ContactTitle);

            Assert.Equal("2732 Baker Blvd.", customerModel.Location.Address);
        }

        [Fact]
        public void CustomerModelWithCustomerId_CustomerModelToCustomer_ReturnCustomerWithIdZero()
        {
            var customerModel = new CustomerModel
            {
                CustomerId = 24,
                CompanyName = "Around the Horn",
                Location = new LocationModel { Address = "35 King George" }
            };

            var customer = _mapper.Map<Customer>(customerModel);

            Assert.Equal(0, customer.CustomerId);
            Assert.Equal("Around the Horn", customer.CompanyName);

            Assert.Equal(0, customer.Location.LocationId);
            Assert.Equal("35 King George", customer.Location.Address);
        }
    }
}