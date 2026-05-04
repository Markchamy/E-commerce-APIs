using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("admin/api/2024-01")]
    public class PurchaseOrderController : ControllerBase
    {
        private readonly IPurchaseOrderServices _purchaseOrderServices;
        public PurchaseOrderController(IPurchaseOrderServices purchaseOrderServices)
        {
            _purchaseOrderServices = purchaseOrderServices;
        }

        [HttpPost("supplier")]
        public async Task<IActionResult> CreateSupplier([FromBody] SupplierDTO supplierDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ResponseBase.Failure("Invalid data provided"));
            }

            var result = await _purchaseOrderServices.AddSupplier(supplierDTO);

            if (!result.IsSuccess)
            {
                return Conflict(result);
            }

            return Ok(result);
        }

        [HttpGet("supplier")]
        public async Task<IActionResult> GetAllSuppliers()
        {
            var result = await _purchaseOrderServices.GetAllSuppliers();

            if (!result.IsSuccess)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpPut("supplier/{id}")]
        public async Task<IActionResult> UpdateSupplier(int id, [FromBody] SupplierDTO supplierDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ResponseBase.Failure("Invalid data provided"));
            }

            var result = await _purchaseOrderServices.UpdateSupplier(id, supplierDTO);

            if (!result.IsSuccess)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
    }
}
