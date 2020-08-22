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
    public class ShipperService : IShipperService
    {
        private readonly IRepository<Shipper> _shipperRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly ILogger<ShipperService> _logger;

        public ShipperService(IRepository<Shipper> shipperRepository,
                              IRepository<Order> orderRepository,
                              ILogger<ShipperService> logger)
        {
            _shipperRepository = shipperRepository;
            _orderRepository = orderRepository;
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

        public async Task Delete(Shipper shipper)
        {
            _logger.LogInformation($"Detach order from shipper: {shipper.CompanyName}");

            await _orderRepository.FindByCondition(order => order.ShipperId == shipper.ShipperId)
                                  .ForEachAsync(DetachOrderFromShipper);

            _logger.LogInformation($"Deleting a shipper: {shipper.CompanyName}");
            _shipperRepository.Delete(shipper);
        }

        public async Task<bool> IsSavedToDb()
        {
            return await _shipperRepository.SaveChangesAsync();
        }

        public async Task<Order> GetOrderById(int orderId)
        {
            return await _orderRepository.FindByCondition(order => order.OrderId == orderId)
                                         .FirstOrDefaultAsync();
        }

        public async Task<ICollection<Order>> GetAllEntities(int shipperId)
        {
            _logger.LogInformation($"Retrieving orders of shipper with id: {shipperId}");
            return await _orderRepository.FindByCondition(order => order.ShipperId == shipperId)
                                         .ToArrayAsync();
        }

        public async Task<Order> GetEntityById(int shipperId, int orderId)
        {
            _logger.LogInformation($"Retrieving order with id {orderId} " +
                                   $"of shipper with id: {shipperId}");
            return await QueryOrderById(shipperId, orderId).FirstOrDefaultAsync();
        }

        public void UpdateEntity(int shipperId, Order order)
        {
            _logger.LogInformation($"Updating order with id {order.OrderId} " +
                                   $"of shipper with id: {shipperId}");

            // Attach order to shipper
            order.ShipperId = shipperId;
            _orderRepository.Update(order);
        }

        public void DeleteEntity(int shipperId, Order order)
        {
            _logger.LogInformation($"Deleting order with id {order.OrderId} " +
                                   $"of shipper with id: {shipperId}");

            // Detach order from shipper, but retain the order in DB
            DetachOrderFromShipper(order);
        }

        private IQueryable<Order> QueryOrderById(int shipperId, int orderId)
        {
            Expression<Func<Order,bool>> filterByOrderAndShipperIds =
                order => order.OrderId == orderId && order.ShipperId == shipperId;

            return _orderRepository.FindByCondition(filterByOrderAndShipperIds);
        }

        private static void DetachOrderFromShipper(Order order)
        {
            order.ShipperId = null;
            order.ShippedDate = null;
            order.ShipName = null;
        }
    }
}