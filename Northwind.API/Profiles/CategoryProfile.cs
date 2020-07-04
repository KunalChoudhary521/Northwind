using AutoMapper;
using Northwind.API.Models;
using Northwind.Data.Entities;

namespace Northwind.API.Profiles
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<Category, CategoryModel>()
                .ReverseMap() // reverse map to create an entity from model (ex. via HTTP POST)
                .ForMember(c => c.CategoryId, opt => opt.Ignore()); // ignore id provided in request
        }
    }
}