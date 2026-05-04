namespace Backend.Models.DTOs
{
    public class CreateAdjustmentRequest
    {
        public string ActivityType { get; set; } = string.Empty;
        public string? ActivityReference { get; set; }
        public string? ActivityDescription { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public long? CreatedById { get; set; }
        public int UnavailableChange { get; set; }
        public int CommittedChange { get; set; }
        public int AvailableChange { get; set; }
        public int OnHandChange { get; set; }
        public int IncomingChange { get; set; }
        public string? Notes { get; set; }
    }

    public class AdjustmentHistoryResponse
    {
        public bool IsSuccess { get; set; }
        public AdjustmentHistoryData? Data { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class AdjustmentHistoryData
    {
        public List<AdjustmentDto> Adjustments { get; set; } = new();
        public PaginationInfo Pagination { get; set; } = new();
    }

    public class AdjustmentDto
    {
        public long Id { get; set; }
        public long VariantId { get; set; }
        public long ProductId { get; set; }
        public string ActivityType { get; set; } = string.Empty;
        public string? ActivityReference { get; set; }
        public string? ActivityDescription { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public long? CreatedById { get; set; }
        public int Unavailable { get; set; }
        public int Committed { get; set; }
        public int Available { get; set; }
        public int OnHand { get; set; }
        public int Incoming { get; set; }
        public int UnavailableChange { get; set; }
        public int CommittedChange { get; set; }
        public int AvailableChange { get; set; }
        public int OnHandChange { get; set; }
        public int IncomingChange { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PaginationInfo
    {
        public int CurrentPage { get; set; }
        public int PerPage { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
    }

    public class CreateAdjustmentResponse
    {
        public bool IsSuccess { get; set; }
        public CreateAdjustmentData? Data { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class CreateAdjustmentData
    {
        public long AdjustmentId { get; set; }
        public long VariantId { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class InventoryStatusResponse
    {
        public bool IsSuccess { get; set; }
        public InventoryStatusData? Data { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class InventoryStatusData
    {
        public long VariantId { get; set; }
        public int Unavailable { get; set; }
        public int Committed { get; set; }
        public int Available { get; set; }
        public int OnHand { get; set; }
        public int Incoming { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
