using Northwind.Data.Contexts;
using Northwind.Data.Entities;

namespace Northwind.API.Repositories
{
    public class CategoryRepository : RepositoryBase<Category>
    {
        public CategoryRepository(NorthwindContext context) : base(context) { }
    }
}