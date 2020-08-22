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
    public class ShippersController : ControllerBase
    {
        private readonly IShipperService _shipperService;
        private readonly IMapper _mapper;

        public ShippersController(IShipperService shipperService, IMapper mapper)
        {
            _shipperService = shipperService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<ShipperModel[]>> GetShippers()
        {
            var shippers = await _shipperService.GetAll();

            if (shippers == null)
                return NotFound();

            return _mapper.Map<ShipperModel[]>(shippers);
        }

        [HttpGet("{shipperId:int}")]
        public async Task<ActionResult<ShipperModel>> GetShipper(int shipperId)
        {
            var shipper = await _shipperService.GetById(shipperId);

            if (shipper == null)
                return NotFound();

            return _mapper.Map<ShipperModel>(shipper);
        }

        [HttpPost]
        public async Task<ActionResult<ShipperModel>> AddShipper(ShipperModel shipperModel)
        {
            var shipper = _mapper.Map<Shipper>(shipperModel);
            _shipperService.Add(shipper);

            if (await _shipperService.IsSavedToDb())
            {
                var persistedShipperModel = _mapper.Map<ShipperModel>(shipper);
                return CreatedAtAction(nameof(GetShipper),
                                       new { shipperId = persistedShipperModel.ShipperId },
                                       persistedShipperModel);
            }

            return BadRequest();
        }

        [HttpPut("{shipperId:int}")]
        public async Task<ActionResult<ShipperModel>> UpdateShipper(int shipperId, ShipperModel shipperModel)
        {
            var oldShipper = await _shipperService.GetById(shipperId);
            if (oldShipper == null)
                return NotFound();

            var newShipper = _mapper.Map(shipperModel, oldShipper);
            _shipperService.Update(newShipper);

            if (await _shipperService.IsSavedToDb())
                return Ok(_mapper.Map<ShipperModel>(newShipper));

            return BadRequest();
        }

        [HttpDelete("{shipperId:int}")]
        public async Task<ActionResult> DeleteShipper(int shipperId)
        {
            var existingShipper = await _shipperService.GetById(shipperId);
            if (existingShipper == null)
                return NotFound();

            await _shipperService.Delete(existingShipper);

            if (await _shipperService.IsSavedToDb())
                return Ok($"Shipper with id '{shipperId}' has been deleted");

            return BadRequest();
        }

        [HttpGet("{shipperId:int}/orders")]
        public async Task<ActionResult<OrderResponseModel[]>> GetShipperOrders(int shipperId)
        {
            await DoesShipperExist(shipperId);

            var orders = await _shipperService.GetAllEntities(shipperId);
            return _mapper.Map<OrderResponseModel[]>(orders);
        }

        [HttpGet("{shipperId:int}/orders/{orderId:int}")]
        public async Task<ActionResult<OrderResponseModel>> GetShipperOrder(int shipperId, int orderId)
        {
            await DoesShipperExist(shipperId);

            var order = await GetOrder(shipperId, orderId);
            return _mapper.Map<OrderResponseModel>(order);
        }

        [HttpPut("{shipperId:int}/orders/{orderId:int}")]
        public async Task<ActionResult<OrderResponseModel>> UpdateShipperOrder(int shipperId, int orderId,
                                                                               ShipperOrderModel orderModel)
        {
            await DoesShipperExist(shipperId);

            var oldOrder = await _shipperService.GetOrderById(orderId);
            if(oldOrder == null)
                throw new ProblemDetailsException(StatusCodes.Status404NotFound,
                                                  $"Order with id {orderId} not found");

            var newOrder = _mapper.Map(orderModel, oldOrder);
            _shipperService.UpdateEntity(shipperId, newOrder);

            if (await _shipperService.IsSavedToDb())
                return Ok(_mapper.Map<OrderResponseModel>(newOrder));

            return BadRequest();
        }

        [HttpDelete("{shipperId:int}/orders/{orderId:int}")]
        public async Task<ActionResult<OrderResponseModel>> DeleteShipperOrder(int shipperId, int orderId)
        {
            await DoesShipperExist(shipperId);

            var existingOrder = await GetOrder(shipperId, orderId);

            _shipperService.DeleteEntity(shipperId, existingOrder);

            if (await _shipperService.IsSavedToDb())
                return Ok($"Order with id '{orderId}' of shipper " +
                          $"with id {shipperId} has been deleted");

            return BadRequest();
        }

        private async Task DoesShipperExist(int shipperId)
        {
            if (await _shipperService.GetById(shipperId) == null)
                throw new ProblemDetailsException(StatusCodes.Status404NotFound,
                                                  $"Shipper with id {shipperId} not found");
        }

        private async Task<Order> GetOrder(int shipperId, int orderId)
        {
            var order = await _shipperService.GetEntityById(shipperId, orderId);

            if (order == null)
                throw new ProblemDetailsException(StatusCodes.Status404NotFound,
                                                  $"Order with id {orderId} not found");
            return order;
        }
    }
}