using System.Threading.Tasks;
using AutoMapper;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Northwind.API.Models;
using Northwind.API.Models.Orders;
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

        [HttpGet("{customerId:int}/orders")]
        public async Task<ActionResult<OrderResponseModel[]>> GetCustomerOrders(int customerId)
        {
            await DoesCustomerExist(customerId);

            var orders = await _customerService.GetAllEntities(customerId);
            return _mapper.Map<OrderResponseModel[]>(orders);
        }

        [HttpGet("{customerId:int}/orders/{orderId:int}")]
        public async Task<ActionResult<OrderResponseModel>> GetCustomerOrder(int customerId, int orderId)
        {
            await DoesCustomerExist(customerId);

            var order = await GetOrder(customerId, orderId);

            return _mapper.Map<OrderResponseModel>(order);
        }

        [HttpPost("{customerId:int}/orders")]
        public async Task<ActionResult<OrderResponseModel>> AddCustomerOrder(int customerId,
                                                                             OrderRequestModel orderRequestModel)
        {
            var customer = await DoesCustomerExist(customerId);

            var order = _mapper.Map<Order>(orderRequestModel);
            await _customerService.AddEntity(customer, order);
            if (await _customerService.IsSavedToDb())
            {
                var persistedOrderModel = _mapper.Map<OrderResponseModel>(order);
                return CreatedAtAction(nameof(GetCustomerOrder),
                                       new { customerId, orderId = order.OrderId },
                                       persistedOrderModel);
            }

            return BadRequest();
        }

        [HttpPut("{customerId:int}/orders/{orderId:int}")]
        public async Task<ActionResult<OrderResponseModel>> UpdateCustomerOrder(int customerId, int orderId,
                                                                                OrderModelBase orderRequest)
        {
            await DoesCustomerExist(customerId);

            var order = await GetOrder(customerId, orderId);

            if (order.ShippedDate != null)
                return BadRequest("Order's Required data cannot be updated as " +
                                  $"it has already been scheduled to ship on {order.ShippedDate}");

            order.RequiredDate = orderRequest.RequiredDate;
            _customerService.UpdateEntity(customerId, order);

            if (await _customerService.IsSavedToDb())
                return Ok(_mapper.Map<OrderResponseModel>(order));

            return BadRequest();
        }

        [HttpDelete("{customerId:int}/orders/{orderId:int}")]
        public async Task<ActionResult> DeleteCustomerOrder(int customerId, int orderId)
        {
            await DoesCustomerExist(customerId);

            var existingOrder = await GetOrder(customerId, orderId);

            _customerService.DeleteEntity(customerId, existingOrder);

            if (await _customerService.IsSavedToDb())
                return Ok($"Order with id '{orderId}' of customer " +
                          $"with id {customerId} has been deleted");

            return BadRequest();
        }

        private async Task<Customer> DoesCustomerExist(int customerId)
        {
            var customer = await _customerService.GetById(customerId);
            if(customer  == null)
                throw new ProblemDetailsException(StatusCodes.Status404NotFound,
                                                  $"Customer with id {customerId} not found");

            return customer;
        }

        private async Task<Order> GetOrder(int customerId, int orderId)
        {
            var order = await _customerService.GetEntityById(customerId, orderId);

            if (order == null)
                throw new ProblemDetailsException(StatusCodes.Status404NotFound,
                                                  $"Order with id {orderId} not found");

            return order;
        }
    }
}