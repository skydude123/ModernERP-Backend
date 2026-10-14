using System.ComponentModel.DataAnnotations;

namespace HRIStwinbeeAPI.DTO.Depreciation;

public sealed class DepreciationRunQueryDto
{
    public string? PeriodFrom { get; set; }
    public string? PeriodTo { get; set; }
    public DateTime? RunDateFrom { get; set; }
    public DateTime? RunDateTo { get; set; }
    public string Status { get; set; } = "ALL";
    public string? BranchCode { get; set; }
    public string? AssetCategoryCode { get; set; }
    public string? MethodCode { get; set; }
    public string? Search { get; set; }
    [Range(0, int.MaxValue)] public int Skip { get; set; }
    [Range(1, 200)] public int Take { get; set; } = 50;
    public string SortField { get; set; } = "runDate";
    public string SortDir { get; set; } = "desc";
}

public sealed class DepreciationRunListItemDto
{
    public string DepRunNo { get; set; } = string.Empty;
    public string PeriodKey { get; set; } = string.Empty;
    public DateTime RunDate { get; set; }
    public string? BranchCode { get; set; }
    public string? AssetCategoryCode { get; set; }
    public string? MethodCode { get; set; }
    public string Status { get; set; } = string.Empty;
    public int AssetCount { get; set; }
    public decimal TotalAcquisitionCost { get; set; }
    public decimal TotalCurrentDepreciation { get; set; }
    public decimal TotalClosingNetBookValue { get; set; }
    public string? JournalNo { get; set; }
    public string PreparedBy { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
    public string? PostedBy { get; set; }
    public DateTime? PostedAt { get; set; }
    public string? ReversalJournalNo { get; set; }
    public string? Remarks { get; set; }
    public int TotalCount { get; set; }
}

public sealed class DepreciationRunHeaderDto
{
    public string DepRunNo { get; set; } = string.Empty;
    public string PeriodKey { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime RunDate { get; set; }
    public string? BranchCode { get; set; }
    public string? AssetCategoryCode { get; set; }
    public string? MethodCode { get; set; }
    public string DepreciationConvention { get; set; } = "FULL_MONTH";
    public bool IncludeZeroAmount { get; set; }
    public string Status { get; set; } = "DRAFT";
    public int AssetCount { get; set; }
    public decimal TotalAcquisitionCost { get; set; }
    public decimal TotalOpeningAccumulatedDepreciation { get; set; }
    public decimal TotalCurrentDepreciation { get; set; }
    public decimal TotalClosingAccumulatedDepreciation { get; set; }
    public decimal TotalClosingNetBookValue { get; set; }
    public string? JournalNo { get; set; }
    public string? ReversalJournalNo { get; set; }
    public string? Remarks { get; set; }
    public string PreparedBy { get; set; } = string.Empty;
    public string? SubmittedBy { get; set; }
    public string? ApprovedBy { get; set; }
    public string? PostedBy { get; set; }
    public string? ReversedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? PostedAt { get; set; }
    public DateTime? ReversedAt { get; set; }
    public string? RowVersion { get; set; }
}

public class DepreciationPreviewLineDto
{
    public int LineNo { get; set; }
    public long AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetDescription { get; set; } = string.Empty;
    public string? AssetCategoryCode { get; set; }
    public string? BranchCode { get; set; }
    public string? LocationCode { get; set; }
    public string? CustodianCode { get; set; }
    public DateTime? AcquisitionDate { get; set; }
    public DateTime? InServiceDate { get; set; }
    public DateTime? DepreciationStartDate { get; set; }
    public string MethodCode { get; set; } = string.Empty;
    public string Convention { get; set; } = string.Empty;
    public decimal AcquisitionCost { get; set; }
    public decimal ResidualValue { get; set; }
    public int UsefulLifeMonths { get; set; }
    public int PeriodsDepreciatedBefore { get; set; }
    public decimal OpeningAccumulatedDepreciation { get; set; }
    public decimal OpeningNetBookValue { get; set; }
    public decimal DepreciationRate { get; set; }
    public decimal CurrentDepreciation { get; set; }
    public decimal ClosingAccumulatedDepreciation { get; set; }
    public decimal ClosingNetBookValue { get; set; }
    public string? AssetCostAccount { get; set; }
    public string? DepreciationExpenseAccount { get; set; }
    public string? AccumulatedDepreciationAccount { get; set; }
    public bool IsSelected { get; set; }
    public string EligibilityStatus { get; set; } = string.Empty;
    public string? EligibilityMessage { get; set; }
}

public sealed class DepreciationRunDetailDto : DepreciationPreviewLineDto
{
    public string? PostingStatus { get; set; }
    public string? JournalLineReference { get; set; }
}

public sealed class DepreciationGlPreviewLineDto
{
    public int LineNo { get; set; }
    public string? BranchCode { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Narration { get; set; } = string.Empty;
}

public sealed class DepreciationValidationDto
{
    public long? AssetId { get; set; }
    public string? AssetCode { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public sealed class DepreciationPreviewResponseDto
{
    public List<DepreciationPreviewLineDto> Lines { get; set; } = [];
    public List<DepreciationGlPreviewLineDto> GlLines { get; set; } = [];
    public List<DepreciationValidationDto> Validations { get; set; } = [];
    public int EligibleAssetCount { get; set; }
    public int SelectedAssetCount { get; set; }
    public decimal TotalAcquisitionCost { get; set; }
    public decimal TotalOpeningAccumulatedDepreciation { get; set; }
    public decimal TotalCurrentDepreciation { get; set; }
    public decimal TotalClosingAccumulatedDepreciation { get; set; }
    public decimal TotalClosingNetBookValue { get; set; }
    public decimal GlDifference { get; set; }
}

public sealed class DepreciationRunDto
{
    public DepreciationRunHeaderDto Header { get; set; } = new();
    public List<DepreciationRunDetailDto> Details { get; set; } = [];
    public List<DepreciationGlPreviewLineDto> GlLines { get; set; } = [];
    public List<DepreciationValidationDto> Validations { get; set; } = [];
}

public sealed class DepreciationPreviewRequestDto
{
    [Required, RegularExpression(@"^\d{6}$")] public string PeriodKey { get; set; } = string.Empty;
    public string? BranchCode { get; set; }
    public string? AssetCategoryCode { get; set; }
    public string? MethodCode { get; set; }
    [Required, RegularExpression("^(FULL_MONTH|NEXT_MONTH)$")]
    public string Convention { get; set; } = "FULL_MONTH";
    public bool IncludeZeroAmount { get; set; }
    public List<long> SelectedAssetIds { get; set; } = [];
}

public sealed class DepreciationSaveRequestDto
{
    public string? DepRunNo { get; set; }
    [Required] public DepreciationPreviewRequestDto Setup { get; set; } = new();
    [Required] public DateTime RunDate { get; set; }
    [StringLength(500)] public string? Remarks { get; set; }
    public string? RowVersion { get; set; }
    [Required, MinLength(1)] public List<DepreciationPreviewLineDto> Lines { get; set; } = [];
}

public class DepreciationCommandRequestDto
{
    [Required] public string RowVersion { get; set; } = string.Empty;
    [StringLength(500)] public string? Reason { get; set; }
}

public sealed class DepreciationReverseRequestDto : DepreciationCommandRequestDto
{
    [Required, MinLength(5), StringLength(500)]
    public new string Reason { get; set; } = string.Empty;
}

public sealed class DepreciationCommandResultDto
{
    public string DepRunNo { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? RowVersion { get; set; }
    public string? JournalNo { get; set; }
    public string? ReversalJournalNo { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class DepreciationLookupDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public sealed class DepreciationAssetHistoryDto
{
    public long AssetId { get; set; }
    public string DepRunNo { get; set; } = string.Empty;
    public string PeriodKey { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal CurrentDepreciation { get; set; }
    public decimal ClosingAccumulatedDepreciation { get; set; }
    public decimal ClosingNetBookValue { get; set; }
    public string? JournalNo { get; set; }
    public DateTime? PostedAt { get; set; }
}
