using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Northwind.API.Repositories;
using Northwind.Data.Entities;

namespace Northwind.API.Services
{
    public class ShipperService : IShipperService
    {
        private readonly IRepository<Shipper> _shipperRepository;
        private readonly ILogger<ShipperService> _logger;

        public ShipperService(IRepository<Shipper> shipperRepository,
                              ILogger<ShipperService> logger)
        {
            _shipperRepository = shipperRepository;
            _logger = logger;
        }

        public async Task<ICollection<Shipper>> GetAll()
        {
            _logger.LogInformation("Retrieving all shippers");
            return await _shipperRepository.FindAll().ToArrayAsync();
        }

        public async Task<Shipper> GetById(int shipperId)
        {
            _logger.LogInformation($"Retrieving shipper with id: {shipperId}");
            return await _shipperRepository.FindByCondition(shipper => shipper.ShipperId == shipperId)
                                           .FirstOrDefaultAsync();

        }

        public void Add(Shipper shipper)
        {
            _logger.LogInformation($"Adding a new shipper: {shipper.CompanyName}");
            _shipperRepository.Add(shipper);
        }

        public void Update(Shipper shipper)
        {
            _logger.LogInformation($"Updating an existing shipper: {shipper.CompanyName}");
            _shipperRepository.Update(shipper);
        }

        public Task Delete(Shipper shipper)
        {
            // Detach orders
            _logger.LogInformation($"Deleting a shipper: {shipper.CompanyName}");
            _shipperRepository.Delete(shipper);
            return Task.CompletedTask;
        }

        public async Task<bool> IsSavedToDb()
        {
            return await _shipperRepository.SaveChangesAsync();
        }
    }
}