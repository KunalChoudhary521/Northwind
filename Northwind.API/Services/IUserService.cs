using System.Collections.Generic;
using System.Threading.Tasks;
using Northwind.Data.Entities;

namespace Northwind.API.Services
{
    public interface IUserService
    {
        Task<ICollection<User>> GetAll();
        Task<User> GetById(int userId);
        Task<User> GetByUserName(string userName);
        void Add(User user, string password);
        void Delete(User user);
        Task<bool> IsSavedToDb();
    }
}