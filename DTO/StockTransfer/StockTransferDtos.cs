using System.ComponentModel.DataAnnotations;

namespace HRIStwinbeeAPI.DTO.StockTransfer;

public sealed class StockTransferQueryRequest
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    [RegularExpression("^(ALL|N|U)$")] public string Status { get; set; } = "ALL";
    [StringLength(5)] public string? SourceWhseCode { get; set; }
    [StringLength(5)] public string? DestinationWhseCode { get; set; }
    [StringLength(100)] public string? Search { get; set; }
    [Range(0, int.MaxValue)] public int Skip { get; set; }
    [Range(1, 500)] public int Take { get; set; } = 50;
    [StringLength(10)]
    public string? UserId { get; set; }
}

public sealed class StockTransferListItemDto
{
    public string StNo { get; set; } = string.Empty;
    public DateTime StDate { get; set; }
    public string SourceWhseCode { get; set; } = string.Empty;
    public string SourceWhseDesc { get; set; } = string.Empty;
    public string DestinationWhseCode { get; set; } = string.Empty;
    public string DestinationWhseDesc { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public string Status { get; set; } = "N";
    public string? ReceivedBy { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public int LineCount { get; set; }
    public decimal TotalBaseQty { get; set; }
    public decimal TotalTransferCost { get; set; }
    public long TotalCount { get; set; }
}

public sealed class StockTransferDocumentDto
{
    public string StNo { get; set; } = string.Empty;
    public DateTime StDate { get; set; }
    public string SourceWhseCode { get; set; } = string.Empty;
    public string SourceWhseDesc { get; set; } = string.Empty;
    public string DestinationWhseCode { get; set; } = string.Empty;
    public string DestinationWhseDesc { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public string Status { get; set; } = "N";
    public string? ReceivedBy { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public string? PostedBy { get; set; }
    public DateTime? PostedAt { get; set; }
    public int FiscalYear { get; set; }
    public int FiscalPeriod { get; set; }
    public string? UserId { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public List<StockTransferDetailDto> Details { get; set; } = [];
}

public sealed class StockTransferDetailDto
{
    public int StSeq { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemDesc { get; set; } = string.Empty;
    public decimal QtyIss { get; set; }
    public string XferUm { get; set; } = string.Empty;
    public decimal ConvFactor { get; set; }
    public string BaseUm { get; set; } = string.Empty;
    public decimal BaseQty { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TransferCost { get; set; }
}

public sealed class UpsertStockTransferRequest
{
    [StringLength(10)] public string? StNo { get; set; }
    public string? ExpectedRowVersion { get; set; }
    [Required] public DateTime StDate { get; set; }
    [Required, StringLength(5)] public string SourceWhseCode { get; set; } = string.Empty;
    [Required, StringLength(5)] public string DestinationWhseCode { get; set; } = string.Empty;
    [Required, StringLength(20)] public string Reference { get; set; } = string.Empty;
    [StringLength(10)] public string? ReceivedBy { get; set; }
    public DateTime? ReceivedDate { get; set; }
    [StringLength(500)] public string? Remarks { get; set; }
    [Required, RegularExpression("^[NU]$")] public string TargetStatus { get; set; } = "N";
    [StringLength(10)] public string? UserId { get; set; }
    [Required, MinLength(1)] public List<StockTransferDetailUpsertDto> Details { get; set; } = [];
}

public sealed class StockTransferDetailUpsertDto
{
    [Range(1, int.MaxValue)] public int StSeq { get; set; }
    [Required, StringLength(20)] public string ItemCode { get; set; } = string.Empty;
    [Range(typeof(decimal), "0.0001", "99999999999999")] public decimal QtyIss { get; set; }
    [Required, StringLength(3)] public string XferUm { get; set; } = string.Empty;
    [Range(typeof(decimal), "0.0001", "99999999999999")] public decimal ConvFactor { get; set; } = 1;
    [Required, StringLength(3)] public string BaseUm { get; set; } = string.Empty;
}

public sealed class StockTransferSaveResultDto
{
    public string StNo { get; set; } = string.Empty;
    public string Status { get; set; } = "N";
    public byte[] RowVersion { get; set; } = [];
    public string Message { get; set; } = string.Empty;
}

public sealed class StockTransferWarehouseDto
{
    public string WhseCode { get; set; } = string.Empty;
    public string WhseDesc { get; set; } = string.Empty;
}

public sealed class StockTransferSourceItemDto
{
    public string ItemCode { get; set; } = string.Empty;
    public string ItemDesc { get; set; } = string.Empty;
    public string SourceWhseCode { get; set; } = string.Empty;
    public decimal StockOnHand { get; set; }
    public string BaseUm { get; set; } = string.Empty;
    public decimal UnitCost { get; set; }
}

public sealed class StockTransferPagedResult<T>
{
    public IReadOnlyList<T> Data { get; init; } = [];
    public long Total { get; init; }
}

public sealed class ReverseStockTransferRequest
{
    [StringLength(10)]
    public string? StNo { get; set; }

    public string? ExpectedRowVersion { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 5)]
    public string Reason { get; set; } = string.Empty;

    [StringLength(10)]
    public string? UserId { get; set; }
}


public sealed class StockTransferReverseResultDto
{
    public string StNo { get; set; } = string.Empty;

    public string Status { get; set; } = "N";

    public byte[] RowVersion { get; set; } = [];

    public string Message { get; set; } = string.Empty;
}
