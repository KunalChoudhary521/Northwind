using Northwind.Data.Contexts;
using Northwind.Data.Entities;

namespace Northwind.API.Repositories
{
    public class ProductRepository : RepositoryBase<Product>
    {
        public ProductRepository(NorthwindContext context) : base(context) { }
    }
}