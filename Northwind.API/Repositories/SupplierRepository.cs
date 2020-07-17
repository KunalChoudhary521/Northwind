using Northwind.Data.Contexts;
using Northwind.Data.Entities;

namespace Northwind.API.Repositories
{
    public class SupplierRepository : RepositoryBase<Supplier>
    {
        public SupplierRepository(NorthwindContext context) : base(context)
        {
        }
    }
}