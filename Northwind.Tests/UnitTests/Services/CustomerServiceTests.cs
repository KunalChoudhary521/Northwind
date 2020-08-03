using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Northwind.API.Repositories;
using Northwind.API.Services;
using Northwind.Data.Entities;
using Xunit;

namespace Northwind.Tests.UnitTests.Services
{
    public class CustomerServiceTests
    {
        private readonly Mock<IRepository<Location>> _locationRepository;
        private readonly Mock<IRepository<Customer>> _customerRepository;
        private readonly ICustomerService _customerService;

        public CustomerServiceTests()
        {
            _customerRepository = new Mock<IRepository<Customer>>();
            _locationRepository = new Mock<IRepository<Location>>();
            var logger = new Mock<ILogger<CustomerService>>();

            _customerService = new CustomerService(_customerRepository.Object,
                                                   _locationRepository.Object,
                                                   logger.Object);
        }

        [Fact]
        public async Task CustomerWithLocation_Delete_DeleteCustomerAndLocation()
        {
            var customer = new Customer { Location = new Location() };

            await _customerService.Delete(customer);

            _customerRepository.Verify(c => c.Delete(customer));
            _locationRepository.Verify(l => l.Delete(customer.Location));
        }
    }
}