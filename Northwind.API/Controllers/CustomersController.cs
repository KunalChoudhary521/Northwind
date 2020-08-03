using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Northwind.API.Models;
using Northwind.API.Services;
using Northwind.Data.Entities;

namespace Northwind.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IMapper _mapper;

        public CustomersController(ICustomerService customerService, IMapper mapper)
        {
            _customerService = customerService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<CustomerModel[]>> GetCustomers()
        {
            var customers = await _customerService.GetAll();

            if (customers == null)
                return NotFound();

            return _mapper.Map<CustomerModel[]>(customers);
        }

        [HttpGet("{customerId:int}")]
        public async Task<ActionResult<CustomerModel>> GetCustomer(int customerId)
        {
            var customer = await _customerService.GetById(customerId);

            if (customer == null)
                return NotFound();

            return _mapper.Map<CustomerModel>(customer);
        }

        [HttpPost]
        public async Task<ActionResult<CustomerModel>> AddCustomer(CustomerModel customerModel)
        {
            var customer = _mapper.Map<Customer>(customerModel);
            _customerService.Add(customer);
            if (await _customerService.IsSavedToDb())
            {
                var persistedCustomerModel = _mapper.Map<CustomerModel>(customer);
                return CreatedAtAction(nameof(GetCustomer),
                                       new {customerId = persistedCustomerModel.CustomerId},
                                       persistedCustomerModel);
            }

            return BadRequest();
        }

        [HttpPut("{customerId:int}")]
        public async Task<ActionResult<CustomerModel>> UpdateCustomer(int customerId, CustomerModel customerModel)
        {
            var oldCustomer = await _customerService.GetById(customerId);
            if (oldCustomer == null)
                return NotFound();

            var newCustomer = _mapper.Map(customerModel, oldCustomer);
            _customerService.Update(newCustomer);

            if (await _customerService.IsSavedToDb())
                return Ok(_mapper.Map<CustomerModel>(newCustomer));

            return BadRequest();
        }

        [HttpDelete("{customerId:int}")]
        public async Task<ActionResult> DeleteCustomer(int customerId)
        {
            var existingCustomer = await _customerService.GetById(customerId);
            if (existingCustomer == null)
                return NotFound();

            await _customerService.Delete(existingCustomer);

            if(await _customerService.IsSavedToDb())
                return Ok($"'{existingCustomer.CompanyName}' customer has been deleted");

            return BadRequest();
        }

    }
}