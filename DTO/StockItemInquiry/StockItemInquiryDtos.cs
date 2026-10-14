using System.ComponentModel.DataAnnotations;

namespace HRIStwinbeeAPI.DTO.StockItemInquiry;

public sealed class StockItemListQueryDto
{
    [StringLength(100)] public string? Search { get; set; }
    [StringLength(10)] public string? GroupCode { get; set; }
    [Range(0, int.MaxValue)] public int Skip { get; set; }
    [Range(1, 200)] public int Take { get; set; } = 50;
}

public sealed class StockCardQueryDto
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public sealed class StockItemPagedResultDto<T>
{
    public IReadOnlyList<T> Data { get; init; } = [];
    public long Total { get; init; }
}

public sealed class StockItemDto
{
    public string ItemCode { get; set; } = string.Empty;
    public string ItemDesc { get; set; } = string.Empty;
    public string GroupCode { get; set; } = string.Empty;
    public string GroupDesc { get; set; } = string.Empty;
    public string BaseUm { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty;
    public string? ShortDesc { get; set; }
    public decimal LastUnitCost { get; set; }
}

public sealed class WarehouseStockDto
{
    public string WhseCode { get; set; } = string.Empty;
    public string? BranchName { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public decimal QtyReceived { get; set; }
    public decimal QtyIssued { get; set; }
    public decimal QtyForIssue { get; set; }
    public decimal QtyPo { get; set; }
    public decimal OnHandQty { get; set; }
    public decimal AvailableQty { get; set; }
    public string BaseUm { get; set; } = string.Empty;
    public decimal OnHandCost { get; set; }
    public decimal LastUnitCost { get; set; }
    public string? UserId { get; set; }
    public DateTime? DateChanged { get; set; }
}

public sealed class WarehouseLotDto
{
    public string WhseCode { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string TransType { get; set; } = string.Empty;
    public string TransNo { get; set; } = string.Empty;
    public DateTime TransDate { get; set; }
    public int TransLineNo { get; set; }
    public string? PoNo { get; set; }
    public int? PoLineNo { get; set; }
    public decimal BaseQty { get; set; }
    public decimal QtyIssued { get; set; }
    public decimal RemainingQty { get; set; }
    public string BaseUm { get; set; } = string.Empty;
    public decimal QtyReceived { get; set; }
    public string? PoUm { get; set; }
    public decimal PoConvFactor { get; set; }
    public decimal PrConvFactor { get; set; }
    public decimal UnitCost { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal RemainingValue { get; set; }
    public string? UserId { get; set; }
    public DateTime? DateChanged { get; set; }
}

public sealed class StockCardDto
{
    public int RowNo { get; set; }
    public DateTime TransDate { get; set; }
    public string? TransType { get; set; }
    public string? TransNo { get; set; }
    public int? TransLineNo { get; set; }
    public string? Reference { get; set; }
    public string? PoNo { get; set; }
    public int? PoLineNo { get; set; }
    public string? SuppCode { get; set; }
    public string WhseCode { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public decimal BeginningQty { get; set; }
    public decimal QtyIn { get; set; }
    public decimal QtyOut { get; set; }
    public decimal TransQty { get; set; }
    public decimal EndingQty { get; set; }
    public decimal UnitCost { get; set; }
    public decimal BeginningCost { get; set; }
    public decimal MovementValue { get; set; }
    public string? BaseUm { get; set; }
    public string? PoUm { get; set; }
    public decimal PoConvFactor { get; set; }
    public decimal UnitPrice { get; set; }
    public string? RrLotNo { get; set; }
    public string? RrLotTransType { get; set; }
    public string? WorkCenter { get; set; }
    public string? UserId { get; set; }
    public DateTime? DateChanged { get; set; }
}
