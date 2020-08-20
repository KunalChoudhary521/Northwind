using Northwind.Data.Contexts;
using Northwind.Data.Entities;

namespace Northwind.API.Repositories
{
    public class ShipperRepository : RepositoryBase<Shipper>
    {
        public ShipperRepository(NorthwindContext context) : base(context)
        {
        }
    }
}