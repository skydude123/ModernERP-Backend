using System.ComponentModel.DataAnnotations;

namespace HRIStwinbeeAPI.DTO.PO;

public sealed class PurchaseOrderReportQueryDto
{
    [Required] public string ReportType { get; set; } = "PO_SUMMARY";
    public DateTime? PoDateFrom { get; set; }
    public DateTime? PoDateTo { get; set; }
    public DateTime? DeliveryDateFrom { get; set; }
    public DateTime? DeliveryDateTo { get; set; }
    public string? PoStatus { get; set; }
    public string? BalanceStatus { get; set; }
    public string? PoType { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemDesc { get; set; }
    public string? PoNo { get; set; }
    public string? Purchaser { get; set; }
    public string? BranchName { get; set; }
    [Required] public string SupplierMode { get; set; } = "ALL";
    public string? SupplierCodes { get; set; }
    [Range(0, int.MaxValue)] public int Skip { get; set; }
    [Range(1, 500)] public int Take { get; set; } = 50;
}

public sealed class PurchaseOrderReportRowDto
{
    public string ReportType { get; set; } = string.Empty;
    public string? PoNo { get; set; }
    public DateTime? PoDate { get; set; }
    public DateTime? PoDeliveryDate { get; set; }
    public string? PoType { get; set; }
    public string? PoStatus { get; set; }
    public string? PoStatusName { get; set; }
    public string? SuppCode { get; set; }
    public string? SuppName { get; set; }
    public string? BranchName { get; set; }
    public string? Purchaser { get; set; }
    public int? PoLineNo { get; set; }
    public int? LineCount { get; set; }
    public int? PoCount { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemDesc { get; set; }
    public string? PoUm { get; set; }
    public decimal? QtyPo { get; set; }
    public decimal? QtyFulfilled { get; set; }
    public decimal? BalanceQty { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? PoAmount { get; set; }
    public decimal? FulfilledAmount { get; set; }
    public decimal? BalanceAmount { get; set; }
    public string? BalanceStatus { get; set; }
    public long TotalCount { get; set; }
}

public sealed class PurchaseOrderReportPageDto
{
    public IReadOnlyList<PurchaseOrderReportRowDto> Data { get; init; } = [];
    public long Total { get; init; }
}
