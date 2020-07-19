using AutoMapper;
using Northwind.API.Models;
using Northwind.Data.Entities;

namespace Northwind.API.Profiles
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<Product, ProductModel>()
                .ReverseMap()
                .ForMember(p => p.ProductId, opt => opt.Ignore());
        }
    }
}