using System.Threading.Tasks;
using Northwind.Data.Entities;

namespace Northwind.API.Services
{
    public interface IAuthService
    {
        Task<User> GetByCredentials(string userName, string password);
        Task<User> GetByRefreshToken(string refreshToken);
        User CreateCredentials(User user);
        User RefreshCredentials(User user);
        void RevokeCredentials(User user);
        Task<bool> IsSavedToDb();
    }
}