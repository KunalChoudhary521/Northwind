using System;
using System.Linq;
using System.Linq.Expressions;
using Northwind.Data.Contexts;
using Northwind.Data.Entities;

namespace Northwind.API.Repositories
{
    public class CategoryRepository : RepositoryBase<Category>
    {
        public CategoryRepository(NorthwindContext context) : base(context) { }

        public override IQueryable<Category> FindAll()
        {
            return Context.Categories;
        }

        public override IQueryable<Category> FindByCondition(Expression<Func<Category, bool>> expression)
        {
            return FindAll().Where(expression);
        }
    }
}