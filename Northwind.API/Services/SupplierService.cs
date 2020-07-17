using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Northwind.API.Repositories;
using Northwind.Data.Entities;

namespace Northwind.API.Services
{
    public class SupplierService : IService<Supplier>
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
            return await _supplierRepository.FindByCondition(supplier => supplier.SupplierId == supplierId)
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
    }
}