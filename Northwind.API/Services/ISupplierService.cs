using System.Collections.Generic;
using System.Threading.Tasks;
using Northwind.Data.Entities;

namespace Northwind.API.Services
{
    public interface ISupplierService : IService<Supplier>
    {
        Task<ICollection<Product>> GetAllProducts(int supplierId);
        Task<Product> GetProductById(int supplierId, int productId);
        void AddProduct(int supplierId, Product product);
        void UpdateProduct(int supplierId, Product product);
        void DeleteProduct(int supplierId, Product product);
    }
}