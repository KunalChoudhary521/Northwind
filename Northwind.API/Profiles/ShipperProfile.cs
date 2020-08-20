using AutoMapper;
using Northwind.API.Models;
using Northwind.Data.Entities;

namespace Northwind.API.Profiles
{
    public class ShipperProfile : Profile
    {
        public ShipperProfile()
        {
            CreateMap<Shipper, ShipperModel>()
                .ReverseMap()
                .ForMember(sh => sh.ShipperId, opt => opt.Ignore());
        }
    }
}