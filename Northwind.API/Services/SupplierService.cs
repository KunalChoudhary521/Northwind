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
        private readonly IRepository<Product> _productRepository;
        private readonly ILogger<SupplierService> _logger;

        public SupplierService(IRepository<Supplier> supplierRepository,
                               IRepository<Location> locationRepository,
                               IRepository<Product> productRepository,
                               ILogger<SupplierService> logger)
        {
            _supplierRepository = supplierRepository;
            _locationRepository = locationRepository;
            _productRepository = productRepository;
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
            return await _supplierRepository.FindByCondition(s => s.SupplierId == supplierId)
                                            .Include(supplier => supplier.Location)
                                            .FirstOrDefaultAsync();
        }

        public void Add(Supplier supplier)
        {
            _logger.LogInformation($"Adding a new supplier: {supplier.CompanyName}");
            _supplierRepository.Add(supplier);
        }

        public void Update(Supplier supplier)
        {
            _logger.LogInformation($"Updating an existing supplier: {supplier.CompanyName}");
            _supplierRepository.Update(supplier);
        }

        public async Task Delete(Supplier supplier)
        {
            _logger.LogInformation($"Detach products from supplier: {supplier.CompanyName}");

            await _productRepository.FindByCondition(p => p.SupplierId == supplier.SupplierId)
                                    .ForEachAsync(p => p.SupplierId = null);

            await _productRepository.SaveChangesAsync();

            _logger.LogInformation($"Deleting a supplier: {supplier.CompanyName}");
            _supplierRepository.Delete(supplier);
            _locationRepository.Delete(supplier.Location);
        }

        public async Task<bool> IsSavedToDb()
        {
            return await _supplierRepository.SaveChangesAsync();
        }

        public async Task<ICollection<Product>> GetAllEntities(int supplierId)
        {
            _logger.LogInformation($"Retrieving products of supplier with id: {supplierId}");
            return await _productRepository.FindByCondition(product => product.SupplierId == supplierId)
                                           .ToArrayAsync();
        }

        public async Task<Product> GetEntityById(int supplierId, int productId)
        {
            _logger.LogInformation($"Retrieving product with id {productId} " +
                                   $"of supplier with id: {supplierId}");

            return await QueryProductById(supplierId, productId).FirstOrDefaultAsync();
        }

        public void AddEntity(int supplierId, Product product)
        {
            _logger.LogInformation($"Adding product {product.ProductName}" +
                                   $" to supplier with id: {supplierId}");

            // Attach product to supplier
            product.SupplierId = supplierId;
            _productRepository.Add(product);
        }

        public void UpdateEntity(int supplierId, Product product)
        {
            _logger.LogInformation($"Updating product {product.ProductName}" +
                                   $" from supplier with id: {supplierId}");

            // Attach product to supplier
            product.SupplierId = supplierId;
            _productRepository.Update(product);
        }

        public void DeleteEntity(int supplierId, Product product)
        {
            _logger.LogInformation($"Deleting product with id ${product.ProductId}" +
                                   $" from supplier with id: {supplierId}");

            // Detach product from supplier, but retain the product in DB
            product.SupplierId = null;
        }

        private IQueryable<Product> QueryProductById(int supplierId, int productId)
        {
            Expression<Func<Product,bool>> filterByProductAndSupplierIds =
                product => product.ProductId == productId && product.SupplierId == supplierId;

            return _productRepository.FindByCondition(filterByProductAndSupplierIds);
        }
    }
}