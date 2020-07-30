using System.Threading.Tasks;
using AutoMapper;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Http;
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
        private readonly ISupplierService _supplierService;
        private readonly IMapper _mapper;

        public SuppliersController(ISupplierService supplierService, IMapper mapper)
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
            _supplierService.Add(supplier);
            if (await _supplierService.IsSavedToDb())
            {
                var persistedSupplierModel = _mapper.Map<SupplierModel>(supplier);
                return CreatedAtAction(nameof(GetSupplier),
                                       new {supplierId = persistedSupplierModel.SupplierId},
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

            var newSupplier = _mapper.Map(supplierModel, oldSupplier);
            _supplierService.Update(newSupplier);

            if (await _supplierService.IsSavedToDb())
                return Ok(_mapper.Map<SupplierModel>(newSupplier));

            return BadRequest();
        }

        [HttpDelete("{supplierId:int}")]
        public async Task<ActionResult> DeleteSupplier(int supplierId)
        {
            var existingSupplier = await _supplierService.GetById(supplierId);
            if (existingSupplier == null)
                return NotFound();

            await _supplierService.Delete(existingSupplier);

            if(await _supplierService.IsSavedToDb())
                return Ok($"'{existingSupplier.CompanyName}' supplier has been deleted");

            return BadRequest();
        }

        [HttpGet("{supplierId:int}/products")]
        public async Task<ActionResult<ProductModel[]>> GetSupplierProducts(int supplierId)
        {
            await DoesSupplierExist(supplierId);

            var products = await _supplierService.GetAllEntities(supplierId);
            return _mapper.Map<ProductModel[]>(products);
        }

        [HttpGet("{supplierId:int}/products/{productId:int}")]
        public async Task<ActionResult<ProductModel>> GetSupplierProduct(int supplierId, int productId)
        {
            await DoesSupplierExist(supplierId);

            var product = await GetProduct(supplierId, productId);

            return _mapper.Map<ProductModel>(product);
        }

        [HttpPost("{supplierId:int}/products")]
        public async Task<ActionResult<ProductModel>> AddSupplierProduct(int supplierId,
                                                                         ProductModel productModel)
        {
            await DoesSupplierExist(supplierId);

            var product = _mapper.Map<Product>(productModel);
            _supplierService.AddEntity(supplierId, product);
            if (await _supplierService.IsSavedToDb())
            {
                var persistedProductModel = _mapper.Map<ProductModel>(product);
                return CreatedAtAction(nameof(GetSupplierProduct),
                                       new { supplierId, productId = product.ProductId },
                                       persistedProductModel);
            }

            return BadRequest();
        }

        [HttpPut("{supplierId:int}/products/{productId:int}")]
        public async Task<ActionResult<ProductModel>> UpdateSupplierProduct(int supplierId,
                                                                            int productId,
                                                                            ProductModel productModel)
        {
            await DoesSupplierExist(supplierId);

            var oldProduct = await GetProduct(supplierId, productId);

            var newProduct = _mapper.Map(productModel, oldProduct);
            _supplierService.UpdateEntity(supplierId, newProduct);

            if (await _supplierService.IsSavedToDb())
                return Ok(_mapper.Map<ProductModel>(newProduct));

            return BadRequest();
        }

        [HttpDelete("{supplierId:int}/products/{productId:int}")]
        public async Task<ActionResult<ProductModel>> DeleteSupplierProduct(int supplierId,
                                                                            int productId)
        {
            await DoesSupplierExist(supplierId);

            var existingProduct = await GetProduct(supplierId, productId);

            _supplierService.DeleteEntity(supplierId, existingProduct);

            if(await _supplierService.IsSavedToDb())
                return Ok($"Product with id '{productId}' of supplier " +
                          $"with id {supplierId} has been deleted");

            return BadRequest();
        }

        private async Task DoesSupplierExist(int supplierId)
        {
            if (await _supplierService.GetById(supplierId) == null)
                throw new ProblemDetailsException(StatusCodes.Status404NotFound,
                                                  $"Supplier with id {supplierId} not found");
        }

        private async Task<Product> GetProduct(int supplierId, int productId)
        {
            var product = await _supplierService.GetEntityById(supplierId, productId);

            if (product == null)
                throw new ProblemDetailsException(StatusCodes.Status404NotFound,
                                                  $"Product with id {productId} not found");
            return product;
        }
    }
}