using Microsoft.Extensions.Logging;
using Moq;
using Northwind.API.Repositories;
using Northwind.API.Services;
using Northwind.Data.Entities;
using Xunit;

namespace Northwind.Tests.UnitTests.Services
{
    public class SupplierServiceTests
    {
        private readonly Mock<IRepository<Supplier>> _supplierRepository;
        private readonly Mock<IRepository<Location>> _locationRepository;
        private readonly IService<Supplier> _supplierService;

        public SupplierServiceTests()
        {
            _supplierRepository = new Mock<IRepository<Supplier>>();
            _locationRepository = new Mock<IRepository<Location>>();
            var logger = new Mock<ILogger<IService<Supplier>>>();

            _supplierService = new SupplierService(_supplierRepository.Object,
                                                   _locationRepository.Object, logger.Object);
        }

        [Fact]
        public void Supplier_Delete_DeleteSupplierAndRelatedLocation()
        {
            var supplier = new Supplier { Location = new Location() };

            _supplierService.Delete(supplier);

            _supplierRepository.Verify(s => s.Delete(supplier));
            _locationRepository.Verify(l => l.Delete(supplier.Location));
        }
    }
}