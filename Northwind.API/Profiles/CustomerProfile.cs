using AutoMapper;
using Northwind.API.Models;
using Northwind.Data.Entities;

namespace Northwind.API.Profiles
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            CreateMap<Customer, CustomerModel>()
                .ReverseMap()
                .ForMember(c => c.CustomerId, opt => opt.Ignore());
        }
    }
}