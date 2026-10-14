using System.ComponentModel.DataAnnotations;

namespace HRIStwinbeeAPI.DTO.MaterialIssuance;

public sealed class MaterialIssuanceQueryRequest
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string Status { get; set; } = "ALL";
    public string SourceType { get; set; } = "ALL";
    public string? WhseCode { get; set; }
    public string? Search { get; set; }
    public string? UserId { get; set; }
    [Range(0, int.MaxValue)] public int Skip { get; set; }
    [Range(1, 500)] public int Take { get; set; } = 50;
}

public sealed class MaterialIssuanceListItemDto
{
    public string IssueNo { get; set; } = "";
    public DateTime IssueDate { get; set; }
    public string WhseCode { get; set; } = "";
    public string SourceType { get; set; } = "";
    public string Reference { get; set; } = "";
    public string? SourceNo { get; set; }
    public string Status { get; set; } = "";
    public string EffectiveStatus { get; set; } = "";
    public int LineCount { get; set; }
    public decimal TotalBaseQty { get; set; }
    public decimal TotalAmount { get; set; }
    public string? UserId { get; set; }
    public long TotalCount { get; set; }
}

public sealed class MaterialIssuanceDocumentDto
{
    public string IssueNo { get; set; } = "";
    public DateTime IssueDate { get; set; }
    public string WhseCode { get; set; } = "";
    public string SourceType { get; set; } = "";
    public string Reference { get; set; } = "";
    public string? SourceNo { get; set; }
    public string? SoNo { get; set; }

    public int? CustId { get; set; }
    public string? CustName { get; set; }

    public string? CcCode { get; set; }
    public string? WcCode { get; set; }
    public string? SuppCode { get; set; }
    public string? Remarks { get; set; }
    public string Status { get; set; } = "N";
    public string EffectiveStatus { get; set; } = "N";
    public int FiscalYear { get; set; }
    public int FiscalPeriod { get; set; }
    public string? UserId { get; set; }
    public string? PostedBy { get; set; }
    public DateTime? PostedAt { get; set; }
    public string? RevertedBy { get; set; }
    public DateTime? RevertedWhen { get; set; }
    public string? RevertRemarks { get; set; }
    public int NoOfReverts { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public List<MaterialIssuanceDetailDto> Details { get; set; } = [];
    public List<MaterialIssuanceLotDto> Lots { get; set; } = [];
    public List<MaterialIssuanceGlLineDto> GlLines { get; set; } = [];
}

public sealed class MaterialIssuanceDetailDto
{
    public int IssueSeq { get; set; }
    public string ItemCode { get; set; } = "";
    public string ItemDesc { get; set; } = "";
    public decimal QtyIss { get; set; }
    public string IssueUm { get; set; } = "";
    public string BaseUm { get; set; } = "";
    public decimal ConvFactor { get; set; }
    public decimal BaseQty { get; set; }
    public decimal UnitCost { get; set; }
    public decimal IssueAmount { get; set; }
    public string? Remarks { get; set; }
    public string? DebitAccount { get; set; }
    public string? InventoryAccount { get; set; }
}

public sealed class MaterialIssuanceLotDto
{
    public int IssueSeq { get; set; }
    public int RrLotRowNo { get; set; }
    public string RrNo { get; set; } = "";
    public decimal AllocatedBaseQty { get; set; }
    public decimal UnitCost { get; set; }
    public decimal IssueValue { get; set; }
    public bool IsActive { get; set; }
}

public sealed class MaterialIssuanceGlLineDto
{
    public int LineNo { get; set; }
    public string AccountCode { get; set; } = "";
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string Reference { get; set; } = "";
    public string? Narration { get; set; }
}

public sealed class UpsertMaterialIssuanceRequest
{
    [StringLength(10)] public string? IssueNo { get; set; }
    public string? ExpectedRowVersion { get; set; }
    [Required] public DateTime IssueDate { get; set; }
    [Required, StringLength(5)] public string WhseCode { get; set; } = "WHSE";
    [Required, RegularExpression("^(IPR|PRD|JOB|PRJ|DPT|JOT|DIR)$")] public string SourceType { get; set; } = "PRD";
    [Required, StringLength(20)] public string Reference { get; set; } = "";
    [StringLength(20)] public string? SourceNo { get; set; }
    [StringLength(10)] public string? SoNo { get; set; }

    public int? CustId { get; set; }

    [StringLength(5)] public string? CcCode { get; set; }
    [StringLength(5)] public string? WcCode { get; set; }
    [StringLength(6)] public string? SuppCode { get; set; }
    [StringLength(500)] public string? Remarks { get; set; }
    [Required, RegularExpression("^[NA]$")] public string TargetStatus { get; set; } = "N";
    [StringLength(10)] public string? UserId { get; set; }
    [Required, MinLength(1)] public List<MaterialIssuanceDetailUpsertDto> Details { get; set; } = [];
}

public sealed class MaterialIssuanceDetailUpsertDto
{
    [Range(1, int.MaxValue)] public int IssueSeq { get; set; }
    [Required, StringLength(20)] public string ItemCode { get; set; } = "";
    [Required, StringLength(60)] public string ItemDesc { get; set; } = "";
    [Range(typeof(decimal), "0.0001", "99999999999999")] public decimal QtyIss { get; set; }
    [Required, StringLength(3)] public string IssueUm { get; set; } = "";
    [Required, StringLength(3)] public string BaseUm { get; set; } = "";
    [Range(typeof(decimal), "0.0001", "99999999999999")] public decimal ConvFactor { get; set; } = 1;
    [StringLength(5)] public string? CcCode { get; set; }
    [StringLength(100)] public string? Remarks { get; set; }
}

public sealed class ReverseMaterialIssuanceRequest
{
    [Required] public string ExpectedRowVersion { get; set; } = "";
    [Required, StringLength(500, MinimumLength = 5)] public string Reason { get; set; } = "";
    [StringLength(10)] public string? UserId { get; set; }
}

public sealed class MaterialIssuanceSaveResultDto
{
    public string IssueNo { get; set; } = "";
    public string Status { get; set; } = "";
    public string EffectiveStatus { get; set; } = "";
    public byte[] RowVersion { get; set; } = [];
    public string Message { get; set; } = "";
}

public sealed class MaterialIssuanceSoVerificationDto
{
    public string SoNo { get; set; } = "";
    public int CustomerId { get; set; }
    public string CustName { get; set; } = "";
}

public sealed class MaterialLookupDto
{
    public string ItemCode { get; init; } = string.Empty;

    public string ItemDesc { get; init; } = string.Empty;

    public string WhseCode { get; init; } = string.Empty;

    public string BaseUm { get; init; } = string.Empty;

    public decimal AvailableQty { get; init; }

    public decimal EstimatedCost { get; init; }

    public string? DebitAccount { get; init; }

    public string? InventoryAccount { get; init; }
}
public sealed class FifoPreviewDto
{
    public int RrLotRowNo { get; set; }
    public string RrNo { get; set; } = "";
    public DateTime ReceiptDate { get; set; }
    public decimal AvailableQty { get; set; }
    public decimal UnitCost { get; set; }
    public decimal AvailableValue { get; set; }
}
public sealed class MaterialIssuanceWarehouseDto { public string WhseCode { get; set; } = ""; public string WhseDesc { get; set; } = ""; }
public sealed class MaterialIssuanceSupplierDto { public string SuppCode { get; set; } = ""; public string SuppName { get; set; } = ""; }
public sealed class MaterialIssuanceWorkCenterDto
{
    public string WcCode { get; set; } = "";
    public string WcDesc { get; set; } = "";
}
public sealed class MaterialIssuancePagedResult<T> { public IReadOnlyList<T> Data { get; init; } = []; public long Total { get; init; } }
