using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("admin/api/2024-01")]
    public class RefundController : ControllerBase
    {
        private readonly IRefundServices _refundRepository;

        public RefundController(IRefundServices refundRepository)
        {
            _refundRepository = refundRepository;
        }

        [HttpPost("orders/{orderid}/refunds")]
        public async Task<IActionResult> CreateRefund(long orderid, [FromBody] RefundAndTransactionDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var order = await _refundRepository.GetOrderById(orderid);
                if (order == null)
                {
                    return NotFound($"Order with ID {orderid} is not found");
                }

                var refunds = new RefundModel
                {
                    orderid = orderid,
                    note = dto.Refund.note,
                    user_id = dto.Refund.user_id,
                    restock = dto.Refund.restock,
                };

                // Save RefundModel first to get its ID
                await _refundRepository.CreateRefund(refunds);
                await _refundRepository.SaveChangesAsync();

                // Reference the existing line item — do NOT create a new one
                var refundLine = new RefundLineItemsModel
                {
                    orderid = orderid,
                    line_item_id = dto.RefundLines.line_item_id,
                    refund_id = refunds.id,
                    restock_type = dto.RefundLines.restock_type,
                    quantity = dto.RefundLines.quantity,
                    location_id = dto.RefundLines.location_id,
                    subtotal = dto.RefundLines.subtotal,
                    total_tax = dto.RefundLines.total_tax,
                };

                var transactions = new TransactionsModel
                {
                    orderid = orderid,
                    refund_id = refunds.id,
                    kind = dto.Transactions.kind,
                    gateway = dto.Transactions.gateway,
                    status = dto.Transactions.status,
                    message = dto.Transactions.message,
                    test = dto.Transactions.test,
                    user_id = dto.Transactions.user_id,
                    parent_id = dto.Transactions.parent_id,
                    source_name = dto.Transactions.source_name,
                    amount = dto.Transactions.amount,
                    currency = dto.Transactions.currency,
                    payment_id = dto.Transactions.payment_id,
                    manual_payment_gateway = dto.Transactions.manual_payment_gateway,
                };

                await _refundRepository.CreateRefundLine(refundLine);
                await _refundRepository.CreateTransaction(transactions);
                await _refundRepository.SaveChangesAsync();

                return Ok(new ResponseBase(true, "Refund created successfully.", new { Refund = refunds, Transaction = transactions, RefundLine = refundLine }));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occurred while creating the order: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }



        [HttpGet("refunds")]
        public async Task<IActionResult> GetAllRefunds()
        {
            try
            {
                var refunds = await _refundRepository.GetAllRefunds();
                return Ok(refunds);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occurred while retrieving refunds: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

        [HttpGet("refunds/{id}")]
        public async Task<IActionResult> GetRefundById(int id)
        {
            try
            {
                var refund = await _refundRepository.GetRefundById(id);
                if (refund == null)
                {
                    return NotFound($"Refund with ID {id} not found.");
                }
                return Ok(refund);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occurred while retrieving the refund: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }
    }
}
