using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Northwind.Data.Contexts;

namespace Northwind.API.Repositories
{
    public abstract class RepositoryBase<T> : IRepository<T> where T : class
    {
        private readonly NorthwindContext _context;

        protected RepositoryBase(NorthwindContext context)
        {
            _context = context;
        }

        public virtual IQueryable<T> FindAll() => _context.Set<T>();
        public virtual IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression)
        {
            return FindAll().Where(expression);
        }

        public void Add(T entity) => _context.Set<T>().Add(entity);
        public void Update(T entity) => _context.Set<T>().Update(entity);
        public void Delete(T entity) => _context.Set<T>().Remove(entity);
        public async Task<bool> SaveChangesAsync() => await _context.SaveChangesAsync() > 0;
    }
}