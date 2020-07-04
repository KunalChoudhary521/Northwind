using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Northwind.API.Repositories
{
    public interface IRepository<T>
    {
        IQueryable<T> FindAll();
        IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression);
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<bool> SaveChangesAsync();
    }
}