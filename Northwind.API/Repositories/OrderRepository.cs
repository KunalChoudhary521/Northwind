using Northwind.Data.Contexts;
using Northwind.Data.Entities;

namespace Northwind.API.Repositories
{
    public class OrderRepository : RepositoryBase<Order>
    {
        public OrderRepository(NorthwindContext context) : base(context) { }
    }
}