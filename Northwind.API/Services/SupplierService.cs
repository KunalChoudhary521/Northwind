using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Northwind.API.Repositories;
using Northwind.Data.Entities;

namespace Northwind.API.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly IRepository<Supplier> _supplierRepository;
        private readonly IRepository<Location> _locationRepository;
        private readonly ILogger<IService<Supplier>> _logger;

        public SupplierService(IRepository<Supplier> supplierRepository,
                               IRepository<Location> locationRepository,
                               ILogger<IService<Supplier>> logger)
        {
            _supplierRepository = supplierRepository;
            _locationRepository = locationRepository;
            _logger = logger;
        }

        public async Task<ICollection<Supplier>> GetAll()
        {
            _logger.LogInformation("Retrieving all suppliers");
            // Since there is only 1 location per supplier, it is useful to return it as well
            return await _supplierRepository.FindAll()
                                            .Include(supplier => supplier.Location)
                                            .ToArrayAsync();
        }

        public async Task<Supplier> GetById(int supplierId)
        {
            _logger.LogInformation($"Retrieving supplier with id: {supplierId}");
            return await _supplierRepository.FindByCondition(FindSupplierById(supplierId))
                                            .Include(supplier => supplier.Location)
                                            .FirstOrDefaultAsync();
        }

        public async Task<Supplier> Add(Supplier supplier)
        {
            _logger.LogInformation($"Adding a new supplier: {supplier.CompanyName}");
            _supplierRepository.Add(supplier);
            return await IsSavedToDb() ?
                   await GetById(supplier.SupplierId) :
                   await Task.FromResult<Supplier>(null);
        }

        public void Update(Supplier supplier)
        {
            _logger.LogInformation($"Updating an existing supplier: {supplier.CompanyName}");
            _supplierRepository.Update(supplier);
        }

        public void Delete(Supplier supplier)
        {
            _logger.LogInformation($"Deleting a supplier: {supplier.CompanyName}");
            _supplierRepository.Delete(supplier);
            _locationRepository.Delete(supplier.Location);
        }

        public async Task<bool> IsSavedToDb()
        {
            return await _supplierRepository.SaveChangesAsync();
        }


        public async Task<ICollection<Product>> GetAllProducts(int supplierId)
        {
            _logger.LogInformation($"Retrieving products of supplier with id: {supplierId}");
            return await _supplierRepository.FindByCondition(FindSupplierById(supplierId))
                                            .Select(supplier => supplier.Products)
                                            .FirstOrDefaultAsync();
        }

        public async Task<Product> GetProductById(int supplierId, int productId)
        {
            _logger.LogInformation($"Retrieving product with id: {productId} " +
                                   $"of supplier with id: {supplierId}");

            Expression<Func<Supplier,Product>> findSupplierProductById =
                supplier => supplier.Products.FirstOrDefault(p => p.ProductId == productId);

            return await _supplierRepository.FindByCondition(FindSupplierById(supplierId))
                                            .Select(findSupplierProductById)
                                            .FirstOrDefaultAsync();
        }

        public Task<Product> AddProduct(Product product)
        {
            throw new System.NotImplementedException();
        }

        public void UpdateProduct(Product product)
        {
            throw new System.NotImplementedException();
        }

        public void DeleteProduct(int productId)
        {
            throw new System.NotImplementedException();
        }

        private static Expression<Func<Supplier, bool>> FindSupplierById(int supplierId)
        {
            return supplier => supplier.SupplierId == supplierId;
        }
    }
}