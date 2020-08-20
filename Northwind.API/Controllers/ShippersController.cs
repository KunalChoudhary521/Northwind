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
    }
}