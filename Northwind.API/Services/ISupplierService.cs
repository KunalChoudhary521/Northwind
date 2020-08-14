using Northwind.Data.Entities;

namespace Northwind.API.Services
{
    public interface ISupplierService : IService<Supplier>, IRelatedData<Product>, IAddEntity<Product>
    {
    }
}