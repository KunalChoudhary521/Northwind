using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Northwind.API.Repositories;
using Northwind.Data.Entities;

namespace Northwind.API.Services
{
    public class CustomerService : ICustomerService
    {
        private const int IsDiscontinued = 1;

        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Location> _locationRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(IRepository<Customer> customerRepository,
                               IRepository<Location> locationRepository,
                               IRepository<Order> orderRepository,
                               IRepository<Product> productRepository,
                               ILogger<CustomerService> logger)
        {
            _customerRepository = customerRepository;
            _locationRepository = locationRepository;
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<ICollection<Customer>> GetAll()
        {
            _logger.LogInformation("Retrieving all customers");

            return await _customerRepository.FindAll()
                                            .Include(customer => customer.Location)
                                            .ToArrayAsync();
        }

        public async Task<Customer> GetById(int customerId)
        {
            _logger.LogInformation($"Retrieving customer with id: {customerId}");
            return await _customerRepository.FindByCondition(customer => customer.CustomerId == customerId)
                                            .Include(customer => customer.Location)
                                            .FirstOrDefaultAsync();
        }

        public void Add(Customer customer)
        {
            _logger.LogInformation($"Adding a new customer: {customer.CompanyName}");
            _customerRepository.Add(customer);
        }

        public void Update(Customer customer)
        {
            _logger.LogInformation($"Updating an existing customer: {customer.CompanyName}");
            _customerRepository.Update(customer);
        }

        public async Task Delete(Customer customer)
        {
            var customerOrders = await GetAllEntities(customer.CustomerId);
            foreach (var order in customerOrders)
            {
                DeleteEntity(customer.CustomerId, order);
            }
            _logger.LogInformation($"Deleting a customer: {customer.CompanyName}");
            _locationRepository.Delete(customer.Location);
            _customerRepository.Delete(customer);
        }

        public async Task<bool> IsSavedToDb()
        {
            return await _customerRepository.SaveChangesAsync();
        }

        public async Task<ICollection<Order>> GetAllEntities(int customerId)
        {
            _logger.LogInformation($"Retrieving orders of customer with id: {customerId}");
            return await _orderRepository.FindByCondition(order => order.CustomerId == customerId)
                                         .Include(order => order.OrderDetails)
                                         .ThenInclude(detail => detail.Product)
                                         .ToArrayAsync();
        }

        public async Task<Order> GetEntityById(int customerId, int orderId)
        {
            _logger.LogInformation($"Retrieving order with id {orderId} " +
                                   $"of customer with id: {customerId}");
            return await _orderRepository.FindByCondition(order => order.CustomerId == customerId)
                                         .Include(order => order.OrderDetails)
                                         .ThenInclude(detail => detail.Product)
                                         .FirstOrDefaultAsync(order => order.OrderId == orderId);
        }

        public async Task AddEntity(Customer customer, Order order)
        {
            _logger.LogInformation($"Adding order with id {order.OrderId} " +
                                   $"to customer: {order.Customer}");

            // TODO: get all products with matching productIds at once, put them in dictionary<ProductId, Product>
            // TODO: what if product Ids in the request are not unique?
            var productIdsNotFound = new List<int>();
            foreach (var orderDetail in order.OrderDetails)
            {
                if(orderDetail.Quantity <= 0)
                    throw new ProblemDetailsException(StatusCodes.Status400BadRequest,
                        $"Product id {orderDetail.ProductId} quantity must be more than zero");

                var product = await _productRepository.FindByCondition(p => p.ProductId == orderDetail.ProductId)
                                                      .FirstOrDefaultAsync();

                if(product.ProductId == orderDetail.ProductId &&
                   product.Discontinued != IsDiscontinued)
                {
                    orderDetail.OrderId = order.OrderId;
                    orderDetail.ProductId = product.ProductId;
                    orderDetail.UnitPrice = product.UnitPrice;
                    // TODO: what if product's units in stock is zero? (set stock to reorder level)
                    orderDetail.Quantity = orderDetail.Quantity > product.UnitsInStock
                                                                ? product.UnitsInStock
                                                                : orderDetail.Quantity;
                }
                else
                {
                    productIdsNotFound.Add(orderDetail.ProductId);
                }
            }

            if (productIdsNotFound.Count > 0)
            {
                var productIds = '[' + string.Join(",", productIdsNotFound) + ']';
                throw new ProblemDetailsException(StatusCodes.Status400BadRequest,
                    $"Product(s) not found for the following id(s): {productIds}");
            }

            order.CustomerId = customer.CustomerId;
            order.LocationId = customer.Location.LocationId;
            order.OrderDate = DateTimeOffset.UtcNow;
            order.Total = CalculateOrderTotal(order.OrderDetails);

            _orderRepository.Add(order);
        }

        public void UpdateEntity(int customerId, Order order)
        {
            _logger.LogInformation($"Updating order {order.OrderId} of customer with id: {customerId}");
            _orderRepository.Update(order);
        }

        public void DeleteEntity(int customerId, Order order)
        {
            _logger.LogInformation($"Deleting order with id ${order.OrderId}" +
                                   $" from customer with id: {order.CustomerId}");

            _orderRepository.Delete(order);
        }

        private static decimal CalculateOrderTotal(IEnumerable<OrderDetail> orderDetails)
        {
            return orderDetails.Sum(od => od.Quantity * od.UnitPrice - od.Discount);
        }
    }
}