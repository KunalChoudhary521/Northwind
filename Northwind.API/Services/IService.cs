using System.Collections.Generic;
using System.Threading.Tasks;

namespace Northwind.API.Services
{
    public interface IService<T>
    {
        Task<ICollection<T>> GetAll();
        Task<T> GetById(int entityId);
        Task<T> Add(T category);
        void Update(T entity);
        void Delete(T entity);
        Task<bool> IsSavedToDb();
    }
}