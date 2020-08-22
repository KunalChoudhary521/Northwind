using System.Threading.Tasks;
using Northwind.Data.Entities;

namespace Northwind.API.Services
{
    public interface IShipperService : IService<Shipper>, IRelatedData<Order>
    {
        Task<Order> GetOrderById(int orderId);
    }
}