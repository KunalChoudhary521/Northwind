using System.Threading.Tasks;
using Northwind.Data.Entities;

namespace Northwind.API.Services
{
    public interface ICategoryService : IService<Category>, IRelatedData<Product>
    {
        Task<Category> GetCategoryByName(string categoryName);
    }
}