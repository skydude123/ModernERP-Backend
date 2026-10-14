using System.ComponentModel.DataAnnotations;

namespace HRIStwinbeeAPI.DTO.AssetAcceptance;

public sealed class ApiResponseDto<T>
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public T? Data { get; init; }
    public string? Error { get; init; }

    public static ApiResponseDto<T> FromData(
        T data,
        string? message = null)
    {
        return new ApiResponseDto<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Error = null
        };
    }

    public static ApiResponseDto<T> FromError(
        string error,
        string? message = null)
    {
        return new ApiResponseDto<T>
        {
            Success = false,
            Message = message,
            Data = default,
            Error = error
        };
    }
}

public sealed class AssetAcceptanceQueryDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string Status { get; set; } = "ALL";
    public string? BranchName { get; set; }
    public string? AssetCategoryCode { get; set; }
    public string? Search { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; } = 50;
    public string SortField { get; set; } = "acceptanceDate";
    public string SortDir { get; set; } = "desc";
}

public sealed class AssetAcceptanceListItemDto
{
    public string AssetAcceptanceNo { get; set; } = string.Empty;
    public DateTime AcceptanceDate { get; set; }
    public string PoNo { get; set; } = string.Empty;
    public string SuppCode { get; set; } = string.Empty;
    public string SuppName { get; set; } = string.Empty;
    public string AssetCategoryCode { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int LineCount { get; set; }
    public decimal TotalCost { get; set; }
    public string? Remarks { get; set; }
    public int TotalCount { get; set; }
}

public sealed class PagedResultDto<T>
{
    public IReadOnlyList<T> Data { get; set; } = [];
    public int Total { get; set; }
}

public sealed class AssetAcceptanceHeaderDto
{
    public string AssetAcceptanceNo { get; set; } = string.Empty;
    public DateTime AcceptanceDate { get; set; }
    public string PoNo { get; set; } = string.Empty;
    public string SuppCode { get; set; } = string.Empty;
    public string SuppName { get; set; } = string.Empty;
    public string AssetCategoryCode { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string ReceivedBy { get; set; } = string.Empty;
    public string AcceptedBy { get; set; } = string.Empty;
    public DateTime InServiceDate { get; set; }
    public string AssetLocationCode { get; set; } = string.Empty;
    public string AcquisitionMode { get; set; } = "CASH";
    public decimal DownpaymentAmount { get; set; }
    public decimal FinancedAmount { get; set; }
    public string? FinancingCompany { get; set; }
    public string? LoanReference { get; set; }
    public string Status { get; set; } = "DRAFT";
    public string? Remarks { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? RowVersion { get; set; }
}

public sealed class AssetAcceptanceSaveHeaderDto
{
    public string? AssetAcceptanceNo { get; set; }

    [Required]
    public DateTime AcceptanceDate { get; set; }

    [Required, StringLength(30)]
    public string PoNo { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string SuppCode { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string AssetCategoryCode { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string BranchName { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Department { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string ReceivedBy { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string AcceptedBy { get; set; } = string.Empty;

    [Required]
    public DateTime InServiceDate { get; set; }

    //[Required, StringLength(30)]
    public string AssetLocationCode { get; set; } = string.Empty;

    [Required, RegularExpression("^(NORMAL AP|FINANCED)$")]
    public string AcquisitionMode { get; set; } = "NORMAL AP";

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal DownpaymentAmount { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal FinancedAmount { get; set; }

    [StringLength(150)]
    public string? FinancingCompany { get; set; }

    [StringLength(80)]
    public string? LoanReference { get; set; }

    [StringLength(500)]
    public string? Remarks { get; set; }

    [Required, StringLength(50)]
    public string UserId { get; set; } = string.Empty;

    public string? RowVersion { get; set; }
}

public class AssetAcceptanceDetailSaveDto
{
    [Range(1, int.MaxValue)]
    public int LineNo { get; set; }

    [Range(1, int.MaxValue)]
    public int PoSeq { get; set; }

    [Required, StringLength(50)]
    public string AssetCode { get; set; } = string.Empty;

    [Required, StringLength(255)]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.0001", "999999999")]
    public decimal Qty { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal UnitCost { get; set; }

    [Required, RegularExpression("^(INDIVIDUAL|GROUPED)$")]
    public string TrackingMode { get; set; } = "INDIVIDUAL";

    [StringLength(100)]
    public string? SerialNo { get; set; }

    [StringLength(100)]
    public string? EngineNo { get; set; }

    [StringLength(100)]
    public string? ChassisNo { get; set; }

    [StringLength(50)]
    public string? PlateNo { get; set; }

    [StringLength(500)]
    public string? WarrantyInfo { get; set; }

    [Required, StringLength(30)]
    public string ConditionStatus { get; set; } = "GOOD";

    [StringLength(500)]
    public string? Remarks { get; set; }

    [Required, StringLength(30)]
    public string DepreciationMethodCode { get; set; } = string.Empty;

    [Range(1, 1200)]
    public int UsefulLifeMonths { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal ResidualValue { get; set; }

    [Required]
    public DateTime DepreciationStartDate { get; set; }

    [Required, StringLength(50)]
    public string AssetCostAccount { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string DepreciationExpenseAccount { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string AccumulatedDepreciationAccount { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string CustodianCode { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string AssetLocationCode { get; set; } = string.Empty;
}

public sealed class AssetAcceptanceDetailDto
    : AssetAcceptanceDetailSaveDto
{
    public decimal OpenPoQty { get; set; }
}

public sealed class AssetAcceptanceDocumentDto
{
    public AssetAcceptanceHeaderDto Header { get; set; } = new();
    public List<AssetAcceptanceDetailDto> Details { get; set; } = [];
}

public sealed class AssetAcceptanceSaveDto
{
    [Required]
    public AssetAcceptanceSaveHeaderDto Header { get; set; } = new();

    [Required, MinLength(1)]
    public List<AssetAcceptanceDetailSaveDto> Details { get; set; } = [];
}

public sealed class WorkflowRequestDto
{
    [Required, StringLength(50)]
    public string UserId { get; set; } = string.Empty;

    public string? RowVersion { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }
}

public sealed class AssetAcceptanceCommandResultDto
{
    public string AssetAcceptanceNo { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? RowVersion { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? AssetCardsCreated { get; set; }
}

public sealed class CodeNameLookupDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public sealed class BranchLookupDto
{
    public string BranchName { get; set; } = string.Empty;
}

public sealed class SupplierLookupDto
{
    public string SuppCode { get; set; } = string.Empty;
    public string SuppName { get; set; } = string.Empty;
}

public sealed class FixedAssetPoLookupDto
{
    public string PoNo { get; set; } = string.Empty;
    public DateTime PoDate { get; set; }
    public string SuppCode { get; set; } = string.Empty;
    public string SuppName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public int OpenLineCount { get; set; }
    public decimal OpenAmount { get; set; }
}

public sealed class FixedAssetPoLineLookupDto
{
    public string PoNo { get; set; } = string.Empty;
    public int PoSeq { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal OrderedQty { get; set; }
    public decimal AcceptedQty { get; set; }
    public decimal OpenQty { get; set; }
    public decimal UnitCost { get; set; }
    public string? AssetCategoryCode { get; set; }
}

public sealed class AssetAccountLookupQueryDto
{
    public string Purpose { get; set; } =
        AssetAccountPurposes.AssetCost;

    public string? Search { get; set; }

    public int Take { get; set; } = 100;
}


public sealed class AssetAccountLookupDto
{
    public string AccountId { get; set; } =
        string.Empty;

    public string AccountName { get; set; } =
        string.Empty;

    public string DisplayText { get; set; } =
        string.Empty;

    public string? AccountType { get; set; }

    public string? NormalBalance { get; set; }

    public string? FSGroup { get; set; }

    public string? FSSubGroup { get; set; }

    public bool IsPosting { get; set; }
}
