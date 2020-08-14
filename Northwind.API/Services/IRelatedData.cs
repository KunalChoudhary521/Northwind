using System.Collections.Generic;
using System.Threading.Tasks;

namespace Northwind.API.Services
{
    public interface IRelatedData<T>
    {
        Task<ICollection<T>> GetAllEntities(int parentId);
        Task<T> GetEntityById(int parentId, int childId);
        void UpdateEntity(int parentId, T child);
        void DeleteEntity(int parentId, T child);
    }
}