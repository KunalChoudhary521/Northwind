using System.Threading.Tasks;
using Northwind.Data.Entities;

namespace Northwind.API.Services
{
    public interface ICategoryService : IService<Category>
    {
        Task<Category> GetCategoryByName(string categoryName);
    }
}