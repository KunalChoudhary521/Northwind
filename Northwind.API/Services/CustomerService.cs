using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Northwind.API.Repositories;
using Northwind.Data.Entities;

namespace Northwind.API.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Location> _locationRepository;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(IRepository<Customer> customerRepository,
                               IRepository<Location> locationRepository,
                               ILogger<CustomerService> logger)
        {
            _customerRepository = customerRepository;
            _locationRepository = locationRepository;
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
            _logger.LogInformation($"Deleting a customer: {customer.CompanyName}");
            _locationRepository.Delete(customer.Location);
            _customerRepository.Delete(customer);
        }

        public async Task<bool> IsSavedToDb()
        {
            return await _customerRepository.SaveChangesAsync();
        }
    }
}