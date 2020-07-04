using System;
using System.Linq;
using System.Linq.Expressions;
using Northwind.Data.Contexts;
using Northwind.Data.Entities;

namespace Northwind.API.Repositories
{
    public class ProductRepository : RepositoryBase<Product>
    {
        public ProductRepository(NorthwindContext context) : base(context) { }

        public override IQueryable<Product> FindAll()
        {
            return Context.Products;
        }

        public override IQueryable<Product> FindByCondition(Expression<Func<Product, bool>> expression)
        {
            return FindAll().Where(expression);
        }

        public IQueryable<Product> FindByCategoryId(int categoryId)
        {
            return FindByCondition(p => p.CategoryId == categoryId);
        }
    }
}