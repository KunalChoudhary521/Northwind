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
    public class SuppliersController : ControllerBase
    {
        private readonly IService<Supplier> _supplierService;
        private readonly IMapper _mapper;

        public SuppliersController(IService<Supplier> supplierService, IMapper mapper)
        {
            _supplierService = supplierService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<SupplierModel[]>> GetSuppliers()
        {
            var suppliers = await _supplierService.GetAll();

            if (suppliers == null)
                return NotFound();

            return _mapper.Map<SupplierModel[]>(suppliers);
        }

        [HttpGet("{supplierId:int}")]
        public async Task<ActionResult<SupplierModel>> GetSupplier(int supplierId)
        {
            var suppliers = await _supplierService.GetById(supplierId);

            if (suppliers == null)
                return NotFound();

            return _mapper.Map<SupplierModel>(suppliers);
        }

        [HttpPost]
        public async Task<ActionResult<SupplierModel>> AddSupplier(SupplierModel supplierModel)
        {
            var supplier = _mapper.Map<Supplier>(supplierModel);
            var persistedSupplier = await _supplierService.Add(supplier);
            if (persistedSupplier != null)
            {
                var persistedSupplierModel = _mapper.Map<SupplierModel>(persistedSupplier);
                return CreatedAtAction("GetSupplier", new {supplierId = persistedSupplierModel.SupplierId},
                                       persistedSupplierModel);
            }

            return BadRequest();
        }

        [HttpPut("{supplierId:int}")]
        public async Task<ActionResult<SupplierModel>> UpdateSupplier(int supplierId, SupplierModel supplierModel)
        {
            var oldSupplier = await _supplierService.GetById(supplierId);
            if (oldSupplier == null)
                return NotFound();

            var updatedSupplier = _mapper.Map(supplierModel, oldSupplier);
            _supplierService.Update(updatedSupplier);

            if (await _supplierService.IsSavedToDb())
                return Ok(_mapper.Map<SupplierModel>(updatedSupplier));

            return BadRequest();
        }

        [HttpDelete("{supplierId:int}")]
        public async Task<ActionResult> DeleteSupplier(int supplierId)
        {
            var existingSupplier = await _supplierService.GetById(supplierId);
            if (existingSupplier == null)
                return NotFound();

            _supplierService.Delete(existingSupplier);

            if(await _supplierService.IsSavedToDb())
                return Ok($"'{existingSupplier.CompanyName}' supplier has been deleted");

            return BadRequest();
        }
    }
}