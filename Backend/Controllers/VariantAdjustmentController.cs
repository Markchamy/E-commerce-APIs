using Backend.Interfaces;
using Backend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("admin/api/2024-01/variants")]
    public class VariantAdjustmentController : ControllerBase
    {
        private readonly IVariantAdjustmentService _variantAdjustmentService;
        private readonly ILogger<VariantAdjustmentController> _logger;

        public VariantAdjustmentController(
            IVariantAdjustmentService variantAdjustmentService,
            ILogger<VariantAdjustmentController> logger)
        {
            _variantAdjustmentService = variantAdjustmentService;
            _logger = logger;
        }

        /// <summary>
        /// Get adjustment history for a variant
        /// </summary>
        /// <param name="variantId">The variant ID</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="perPage">Records per page (default: 25)</param>
        /// <param name="activityType">Filter by activity type (optional)</param>
        /// <param name="startDate">Filter from this date (optional)</param>
        /// <param name="endDate">Filter until this date (optional)</param>
        /// <returns>Adjustment history with pagination</returns>
        [HttpGet("{variantId}/adjustment-history")]
        public async Task<ActionResult<AdjustmentHistoryResponse>> GetAdjustmentHistory(
            long variantId,
            [FromQuery] int page = 1,
            [FromQuery] int perPage = 25,
            [FromQuery] string? activityType = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var (adjustments, totalRecords) = await _variantAdjustmentService.GetAdjustmentHistoryAsync(
                    variantId, page, perPage, activityType, startDate, endDate);

                var totalPages = (int)Math.Ceiling(totalRecords / (double)perPage);

                var response = new AdjustmentHistoryResponse
                {
                    IsSuccess = true,
                    Data = new AdjustmentHistoryData
                    {
                        Adjustments = adjustments.Select(a => new AdjustmentDto
                        {
                            Id = a.Id,
                            VariantId = a.VariantId,
                            ProductId = a.ProductId,
                            ActivityType = a.ActivityType,
                            ActivityReference = a.ActivityReference,
                            ActivityDescription = a.ActivityDescription,
                            CreatedBy = a.CreatedBy,
                            CreatedById = a.CreatedById,
                            Unavailable = a.Unavailable,
                            Committed = a.Committed,
                            Available = a.Available,
                            OnHand = a.OnHand,
                            Incoming = a.Incoming,
                            UnavailableChange = a.UnavailableChange,
                            CommittedChange = a.CommittedChange,
                            AvailableChange = a.AvailableChange,
                            OnHandChange = a.OnHandChange,
                            IncomingChange = a.IncomingChange,
                            Notes = a.Notes,
                            CreatedAt = a.CreatedAt
                        }).ToList(),
                        Pagination = new PaginationInfo
                        {
                            CurrentPage = page,
                            PerPage = perPage,
                            TotalRecords = totalRecords,
                            TotalPages = totalPages
                        }
                    },
                    Message = "Adjustment history retrieved successfully"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving adjustment history for variant {VariantId}", variantId);
                return StatusCode(500, new AdjustmentHistoryResponse
                {
                    IsSuccess = false,
                    Message = "An error occurred while retrieving adjustment history"
                });
            }
        }

        /// <summary>
        /// Create a new adjustment record for a variant
        /// </summary>
        /// <param name="variantId">The variant ID</param>
        /// <param name="request">The adjustment details</param>
        /// <returns>Created adjustment details</returns>
        [HttpPost("{variantId}/adjustment-history")]
        public async Task<ActionResult<CreateAdjustmentResponse>> CreateAdjustment(
            long variantId,
            [FromBody] CreateAdjustmentRequest request)
        {
            try
            {
                // Validate activity type
                var validActivityTypes = new[] { "order_created", "order_fulfilled", "order_edited",
                    "order_cancelled", "manual_adjustment", "transfer", "return" };

                if (!validActivityTypes.Contains(request.ActivityType))
                {
                    return BadRequest(new CreateAdjustmentResponse
                    {
                        IsSuccess = false,
                        Message = $"Invalid activity type. Must be one of: {string.Join(", ", validActivityTypes)}"
                    });
                }

                var adjustment = await _variantAdjustmentService.CreateAdjustmentAsync(
                    variantId,
                    request.ActivityType,
                    request.CreatedBy,
                    request.CreatedById,
                    request.UnavailableChange,
                    request.CommittedChange,
                    request.AvailableChange,
                    request.OnHandChange,
                    request.IncomingChange,
                    request.ActivityReference,
                    request.ActivityDescription,
                    request.Notes);

                var response = new CreateAdjustmentResponse
                {
                    IsSuccess = true,
                    Data = new CreateAdjustmentData
                    {
                        AdjustmentId = adjustment.Id,
                        VariantId = adjustment.VariantId,
                        Message = "Adjustment record created successfully"
                    },
                    Message = "Adjustment record created successfully"
                };

                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Variant {VariantId} not found", variantId);
                return NotFound(new CreateAdjustmentResponse
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid adjustment for variant {VariantId}", variantId);
                return BadRequest(new CreateAdjustmentResponse
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating adjustment for variant {VariantId}", variantId);
                return StatusCode(500, new CreateAdjustmentResponse
                {
                    IsSuccess = false,
                    Message = $"An error occurred while creating the adjustment record: {ex.Message} | Inner: {ex.InnerException?.Message}"
                });
            }
        }

        /// <summary>
        /// Get current inventory status for a variant
        /// </summary>
        /// <param name="variantId">The variant ID</param>
        /// <returns>Current inventory quantities</returns>
        [HttpGet("{variantId}/inventory-status")]
        public async Task<ActionResult<InventoryStatusResponse>> GetInventoryStatus(long variantId)
        {
            try
            {
                var (unavailable, committed, available, onHand, incoming, lastUpdated) =
                    await _variantAdjustmentService.GetCurrentInventoryStatusAsync(variantId);

                var response = new InventoryStatusResponse
                {
                    IsSuccess = true,
                    Data = new InventoryStatusData
                    {
                        VariantId = variantId,
                        Unavailable = unavailable,
                        Committed = committed,
                        Available = available,
                        OnHand = onHand,
                        Incoming = incoming,
                        LastUpdated = lastUpdated
                    },
                    Message = "Inventory status retrieved successfully"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventory status for variant {VariantId}", variantId);
                return StatusCode(500, new InventoryStatusResponse
                {
                    IsSuccess = false,
                    Message = "An error occurred while retrieving inventory status"
                });
            }
        }
    }
}
