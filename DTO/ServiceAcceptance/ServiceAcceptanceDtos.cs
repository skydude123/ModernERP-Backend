using System.ComponentModel.DataAnnotations;

namespace HRIStwinbeeAPI.DTO.ServiceAcceptance;

public sealed class ServiceAcceptanceApiResponseDto<T>
{
    public bool Success { get; init; }

    public string? Message { get; init; }

    public T? Data { get; init; }

    public string? Error { get; init; }

    public static ServiceAcceptanceApiResponseDto<T>
        FromData(
            T data,
            string? message = null)
        =>
        new()
        {
            Success = true,
            Data = data,
            Message = message
        };

    public static ServiceAcceptanceApiResponseDto<T>
        FromError(
            string error)
        =>
        new()
        {
            Success = false,
            Error = error
        };
}


public sealed class ServiceAcceptanceQueryDto
{
    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public string Status { get; set; } =
        "ALL";

    public string? BranchName { get; set; }

    public string? Search { get; set; }

    public int Skip { get; set; }

    public int Take { get; set; } =
        50;

    public string SortField { get; set; } =
        "acceptanceDate";

    public string SortDir { get; set; } =
        "desc";
}


public sealed class ServiceAcceptanceListItemDto
{
    public string ServiceAcceptanceNo { get; set; } =
        string.Empty;

    public DateTime AcceptanceDate { get; set; }

    /*
     * Display value.
     *
     * May contain:
     *   066100, 066125, 066130
     */
    public string PoNo { get; set; } =
        string.Empty;

    public string SuppCode { get; set; } =
        string.Empty;

    public string SuppName { get; set; } =
        string.Empty;

    public string ServiceType { get; set; } =
        string.Empty;

    public string BranchName { get; set; } =
        string.Empty;

    public string? ProjectNo { get; set; }

    public decimal CompletionPercent { get; set; }

    public decimal TotalAcceptedAmount { get; set; }

    public string Status { get; set; } =
        string.Empty;

    public int TotalCount { get; set; }
}


public sealed class ServiceAcceptanceHeaderDto
{
    public string ServiceAcceptanceNo { get; set; } =
        string.Empty;

    public DateTime AcceptanceDate { get; set; }

    /*
     * Display/legacy only.
     * Actual PO belongs to every detail.
     */
    public string? PoNo { get; set; }

    public string SuppCode { get; set; } =
        string.Empty;

    public string SuppName { get; set; } =
        string.Empty;

    public string ServiceType { get; set; } =
        string.Empty;

    public string BranchName { get; set; } =
        string.Empty;

    public string? DepartmentCode { get; set; }

    public string? ProjectNo { get; set; }

    public string? JoNo { get; set; }

    public string? ServiceLocation { get; set; }

    public DateTime? ServicePeriodFrom { get; set; }

    public DateTime? ServicePeriodTo { get; set; }

    public string? RequestedBy { get; set; }

    public string? Remarks { get; set; }

    public decimal CompletionPercent { get; set; }

    public decimal TotalAcceptedAmount { get; set; }

    public string Status { get; set; } =
        string.Empty;

    public string? SubmittedBy { get; set; }

    public DateTime? SubmittedDate { get; set; }

    public string? AcceptedBy { get; set; }

    public DateTime? AcceptedDate { get; set; }

    public string? PostedBy { get; set; }

    public DateTime? PostedDate { get; set; }

    public string? ReversedBy { get; set; }

    public DateTime? ReversedDate { get; set; }

    public string? ReverseReason { get; set; }

    public string CreatedBy { get; set; } =
        string.Empty;

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string RowVersion { get; set; } =
        string.Empty;
}


/* ============================================================
   SAVE HEADER

   IMPORTANT:
   PO NO REMOVED.

   Supplier is now the header driver.
   ============================================================ */

public sealed class ServiceAcceptanceSaveHeaderDto
{
    [StringLength(30)]
    public string? ServiceAcceptanceNo { get; set; }

    [Required]
    public DateTime AcceptanceDate { get; set; }

    [Required]
    [StringLength(10)]
    public string SuppCode { get; set; } =
        string.Empty;

    [Required]
    [RegularExpression(
        "^(REPAIR_MAINTENANCE|MECHANIC_SERVICE|COMPUTER_REPAIR|JOB_OUT|INSTALLATION|LABOR|FREIGHT)$")]
    public string ServiceType { get; set; } =
        "REPAIR_MAINTENANCE";

    [Required]
    [StringLength(50)]
    public string BranchName { get; set; } =
        "MANDAUE";

    [StringLength(30)]
    public string? DepartmentCode { get; set; }

    [StringLength(30)]
    public string? ProjectNo { get; set; }

    [StringLength(30)]
    public string? JoNo { get; set; }

    [StringLength(150)]
    public string? ServiceLocation { get; set; }

    public DateTime? ServicePeriodFrom { get; set; }

    public DateTime? ServicePeriodTo { get; set; }

    [StringLength(100)]
    public string? RequestedBy { get; set; }

    [StringLength(500)]
    public string? Remarks { get; set; }

    [Required]
    [StringLength(50)]
    public string UserId { get; set; } =
        string.Empty;

    public string? RowVersion { get; set; }
}


/* ============================================================
   DETAIL
   ============================================================ */

public class ServiceAcceptanceDetailSaveDto
{
    [Range(1, int.MaxValue)]
    public int LineSeq { get; set; }

    /*
     * NEW:
     * PO belongs to every Service line.
     */
    [Required]
    [StringLength(15)]
    public string PoNo { get; set; } =
        string.Empty;

    [Range(1, int.MaxValue)]
    public int PoSeq { get; set; }

    [StringLength(30)]
    public string? ItemCode { get; set; }

    [Required]
    [StringLength(255)]
    public string ServiceDescription { get; set; } =
        string.Empty;

    [StringLength(20)]
    public string? UnitOfMeasure { get; set; }

    [Range(
        typeof(decimal),
        "0.0001",
        "999999999")]
    public decimal PoQty { get; set; }

    [Range(
        typeof(decimal),
        "0",
        "999999999")]
    public decimal RemainingQty { get; set; }

    [Range(
        typeof(decimal),
        "0.0001",
        "999999999")]
    public decimal AcceptedQty { get; set; }

    [Range(
        typeof(decimal),
        "0",
        "999999999999")]
    public decimal Rate { get; set; }

    [Required]
    [StringLength(10)]
    public string TaxSchemeCode { get; set; } =
        "NV";

    [StringLength(500)]
    public string? Remarks { get; set; }
}


public sealed class ServiceAcceptanceDetailDto
    : ServiceAcceptanceDetailSaveDto
{
    public long ServiceLineNo { get; set; }

    public string ServiceAcceptanceNo { get; set; } =
        string.Empty;

    public decimal AcceptedAmount { get; set; }
}


public class ServiceAcceptanceChecklistSaveDto
{
    [Range(1, int.MaxValue)]
    public int LineSeq { get; set; }

    [Required]
    [StringLength(30)]
    public string ChecklistCode { get; set; } =
        string.Empty;

    [Required]
    [StringLength(200)]
    public string ChecklistDescription { get; set; } =
        string.Empty;

    public bool IsRequired { get; set; } =
        true;

    public bool IsCompleted { get; set; }

    [StringLength(500)]
    public string? Remarks { get; set; }
}


public sealed class ServiceAcceptanceChecklistDto
    : ServiceAcceptanceChecklistSaveDto
{
    public long ChecklistLineNo { get; set; }

    public string ServiceAcceptanceNo { get; set; } =
        string.Empty;

    public string? CompletedBy { get; set; }

    public DateTime? CompletedDate { get; set; }
}


public sealed class ServiceAcceptanceSaveDto
{
    [Required]
    public ServiceAcceptanceSaveHeaderDto Header { get; set; } =
        new();

    [Required]
    [MinLength(1)]
    public List<ServiceAcceptanceDetailSaveDto> Details { get; set; } =
        [];

    public List<ServiceAcceptanceChecklistSaveDto> Checklist { get; set; } =
        [];
}


public sealed class ServiceAcceptanceDocumentDto
{
    public ServiceAcceptanceHeaderDto Header { get; set; } =
        new();

    public List<ServiceAcceptanceDetailDto> Details { get; set; } =
        [];

    public List<ServiceAcceptanceChecklistDto> Checklist { get; set; } =
        [];
}


public sealed class ServiceAcceptanceWorkflowDto
{
    [Required]
    [StringLength(50)]
    public string UserId { get; set; } =
        string.Empty;

    public string? RowVersion { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }
}


public sealed class ServiceAcceptanceCommandResultDto
{
    public string ServiceAcceptanceNo { get; set; } =
        string.Empty;

    public string Status { get; set; } =
        string.Empty;

    public string RowVersion { get; set; } =
        string.Empty;

    public string Message { get; set; } =
        string.Empty;
}


/* ============================================================
   NEW OPEN PO LINE LOOKUP
   ============================================================ */

public sealed class ServiceOpenPoLineLookupDto
{
    public string PoNo { get; set; } =
        string.Empty;

    public DateTime PoDate { get; set; }

    public int PoSeq { get; set; }

    public string SuppCode { get; set; } =
        string.Empty;

    public string SuppName { get; set; } =
        string.Empty;

    public string BranchName { get; set; } =
        string.Empty;

    public string? ItemCode { get; set; }

    public string ServiceDescription { get; set; } =
        string.Empty;

    public string? UnitOfMeasure { get; set; }

    public decimal PoQty { get; set; }

    public decimal AcceptedQty { get; set; }

    public decimal RemainingQty { get; set; }

    public decimal Rate { get; set; }
}


public sealed class ServiceTaxSchemeLookupDto
{
    public string TaxSchemeCode { get; set; } =
        string.Empty;

    public string TaxName { get; set; } =
        string.Empty;
}


public sealed class ServiceSupplierLookupDto
{
    public string SuppCode { get; set; } =
        string.Empty;

    public string SuppName { get; set; } =
        string.Empty;

    public string DisplayText { get; set; } =
        string.Empty;
}


public sealed class ServiceBranchLookupDto
{
    public string BranchName { get; set; } =
        string.Empty;
}


public sealed class ServiceVpSourceDto
{
    public string ServiceAcceptanceNo { get; set; } =
        string.Empty;

    public DateTime AcceptanceDate { get; set; }

    public string PoNo { get; set; } =
        string.Empty;

    public string SuppCode { get; set; } =
        string.Empty;

    public string SuppName { get; set; } =
        string.Empty;

    public string BranchName { get; set; } =
        string.Empty;

    public decimal AcceptedAmount { get; set; }
}


public sealed class ServiceAcceptancePagedResultDto<T>
{
    public IReadOnlyList<T> Data { get; set; } =
        [];

    public int Total { get; set; }
}