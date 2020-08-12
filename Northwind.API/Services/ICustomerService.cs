using System.Threading.Tasks;
using Northwind.Data.Entities;

namespace Northwind.API.Services
{
    public interface ICustomerService : IService<Customer>, IRelatedData<Order>
    {
        Task AddEntity(Customer customer, Order order);
    }
}