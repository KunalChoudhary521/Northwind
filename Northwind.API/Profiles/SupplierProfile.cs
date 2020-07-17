using AutoMapper;
using Northwind.API.Models;
using Northwind.Data.Entities;

namespace Northwind.API.Profiles
{
    public class SupplierProfile : Profile
    {
        public SupplierProfile()
        {
            CreateMap<Supplier, SupplierModel>()
                .ReverseMap()
                .ForMember(s => s.SupplierId, opt => opt.Ignore());
        }
    }
}