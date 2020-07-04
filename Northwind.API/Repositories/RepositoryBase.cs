using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Northwind.Data.Contexts;

namespace Northwind.API.Repositories
{
    public abstract class RepositoryBase<T> : IRepository<T> where T : class
    {
        protected readonly NorthwindContext Context;

        public RepositoryBase(NorthwindContext context)
        {
            Context = context;
        }

        public abstract IQueryable<T> FindAll();
        public abstract IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression);

        public void Add(T entity) => Context.Set<T>().Add(entity);
        public void Update(T entity) => Context.Set<T>().Update(entity);
        public void Delete(T entity) => Context.Set<T>().Remove(entity);
        public async Task<bool> SaveChangesAsync() => await Context.SaveChangesAsync() > 0;
    }
}