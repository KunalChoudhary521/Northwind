using AutoMapper;
using Northwind.API.Models.Orders;
using Northwind.Data.Entities;

namespace Northwind.API.Profiles
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<OrderRequestModel, Order>()
                .ForMember(o => o.OrderDetails, opt => opt.MapFrom(om => om.OrderItems));

            CreateMap<OrderItemRequestModel, OrderDetail>();

            CreateMap<Order, OrderResponseModel>()
                .ForMember(om => om.OrderItems, opt => opt.MapFrom(o => o.OrderDetails));

            CreateMap<OrderDetail, OrderItemResponseModel>()
                .ForMember(oi => oi.ProductName, opt => opt.MapFrom(od => od.Product.ProductName));
        }
    }
}