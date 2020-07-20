using System.Collections.Generic;
using System.Threading.Tasks;

namespace Northwind.API.Services
{
    public interface IService<T>
    {
        Task<ICollection<T>> GetAll();
        Task<T> GetById(int entityId);
        void Add(T category);
        void Update(T entity);
        Task Delete(T entity);
        Task<bool> IsSavedToDb();
    }
}