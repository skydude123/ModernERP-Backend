using System.ComponentModel.DataAnnotations;

namespace HRIStwinbeeAPI.DTO.FuelAcceptance;

public sealed class FuelApiResponseDto<T>
{
    public bool Success { get; init; }

    public string? Message { get; init; }

    public T? Data { get; init; }

    public string? Error { get; init; }

    public static FuelApiResponseDto<T> FromData(
        T data,
        string? message = null) =>
        new()
        {
            Success = true,
            Data = data,
            Message = message
        };

    public static FuelApiResponseDto<T> FromError(
        string error) =>
        new()
        {
            Success = false,
            Error = error
        };
}


/* ================================================================
   LIST QUERY
   ================================================================ */

public sealed class FuelAcceptanceQueryDto
{
    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public string Status { get; set; } = "ALL";

    public string? BranchName { get; set; }

    public string? Search { get; set; }

    public int Skip { get; set; }

    public int Take { get; set; } = 50;

    public string SortField { get; set; } = "acceptanceDate";

    public string SortDir { get; set; } = "desc";
}


/* ================================================================
   LIST ITEM

   IMPORTANT:
   PoNo is now nullable.

   A Fuel Acceptance may contain multiple POs, so header.po_no
   is no longer authoritative.
   ================================================================ */

public sealed class FuelAcceptanceListItemDto
{
    public string FuelAcceptanceNo { get; set; } = string.Empty;

    public DateTime AcceptanceDate { get; set; }

    /*
     * LEGACY / DISPLAY ONLY.
     *
     * One PO:
     *     may contain one PO number.
     *
     * Multiple PO:
     *     may be null.
     */
    public string? PoNo { get; set; }

    public string SuppCode { get; set; } = string.Empty;

    public string SuppName { get; set; } = string.Empty;

    public string BranchName { get; set; } = string.Empty;

    public string VehicleNo { get; set; } = string.Empty;

    public string PlateNo { get; set; } = string.Empty;

    public string DriverName { get; set; } = string.Empty;

    public decimal TotalLiters { get; set; }

    public decimal TotalGross { get; set; }

    public decimal TotalInputVat { get; set; }

    public decimal TotalEwt { get; set; }

    public decimal TotalNet { get; set; }

    public decimal NoReceiptAmount { get; set; }

    public string Status { get; set; } = string.Empty;

    public int TotalCount { get; set; }
}


/* ================================================================
   DOCUMENT HEADER
   ================================================================ */

public sealed class FuelAcceptanceHeaderDto
{
    public string FuelAcceptanceNo { get; set; } = string.Empty;

    public DateTime AcceptanceDate { get; set; }

    /*
     * LEGACY ONLY.
     *
     * Actual PO source is now in each detail line.
     */
    public string? PoNo { get; set; }

    public string SuppCode { get; set; } = string.Empty;

    public string SuppName { get; set; } = string.Empty;

    public string BranchName { get; set; } = string.Empty;

    public string Reference { get; set; } = string.Empty;

    public string VehicleNo { get; set; } = string.Empty;

    public string PlateNo { get; set; } = string.Empty;

    public string DriverName { get; set; } = string.Empty;

    public string Purpose { get; set; } = string.Empty;

    /*
     * Retained temporarily.
     *
     * Your new GetByNo procedure calculates a combined
     * referenced-PO amount.
     */
    public decimal PoAmount { get; set; }

    public string Status { get; set; } = "N";

    public decimal TotalLiters { get; set; }

    public decimal TotalGross { get; set; }

    public decimal TotalBasic { get; set; }

    public decimal TotalInputVat { get; set; }

    public decimal TotalEwt { get; set; }

    public decimal TotalNet { get; set; }

    public decimal NoReceiptAmount { get; set; }

    public string? Remarks { get; set; }

    public string UserId { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? PostedBy { get; set; }

    public DateTime? PostedDate { get; set; }

    public string? ReversedBy { get; set; }

    public DateTime? ReversedDate { get; set; }

    public string? ReverseReason { get; set; }

    public string RowVersion { get; set; } = string.Empty;
}


/* ================================================================
   SAVE HEADER

   NEW DESIGN:

       Header
           Supplier
           Branch
           Acceptance Date

       Detail
           PO No
           PO Line

   PoNo below is retained as OPTIONAL legacy compatibility only.
   Repository no longer uses it as authoritative input.
   ================================================================ */

public sealed class FuelAcceptanceSaveHeaderDto
{
    [StringLength(30)]
    public string? FuelAcceptanceNo { get; set; }

    [Required]
    public DateTime AcceptanceDate { get; set; }

    /*
     * LEGACY ONLY.
     *
     * Do NOT mark Required.
     * New Angular may completely omit this.
     */
    [StringLength(30)]
    public string? PoNo { get; set; }

    /*
     * Supplier is now the primary source selector.
     */
    [Required]
    [StringLength(20)]
    public string SuppCode { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Reference { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string BranchName { get; set; } = "MANDAUE";

    [StringLength(50)]
    public string VehicleNo { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string PlateNo { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string DriverName { get; set; } = string.Empty;

    [StringLength(250)]
    public string Purpose { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Remarks { get; set; }

    public string? RowVersion { get; set; }
}


/* ================================================================
   DETAIL SAVE

   CRITICAL CHANGE:
       PoNo now belongs to every detail.
   ================================================================ */

public class FuelAcceptanceDetailSaveDto
{
    [Range(1, int.MaxValue)]
    public int LineSeq { get; set; }


    /* ============================================================
       MULTI-PO SOURCE
       ============================================================ */

    [Required]
    [StringLength(15)]
    public string PoNo { get; set; } = string.Empty;


    [Required]
    [Range(1, int.MaxValue)]
    public int? PoLineNo { get; set; }


    [StringLength(50)]
    public string? ItemCode { get; set; }


    [StringLength(250)]
    public string? ItemDesc { get; set; }


    public decimal? QtyPo { get; set; }


    /*
     * UI may use this from open-PO lookup.
     *
     * Not sent in the TVP because SQL recalculates availability.
     */
    public decimal? OpenQty { get; set; }


    [StringLength(20)]
    public string? PoUm { get; set; }


    [StringLength(20)]
    public string? BaseUm { get; set; }


    public decimal? PoConvFactor { get; set; }


    /* ============================================================
       INTERNAL FUEL DATA
       ============================================================ */

    [Required]
    public DateTime FuelDate { get; set; }


    /*
     * Hidden in current UI.
     */
    [StringLength(150)]
    public string FuelStation { get; set; } = string.Empty;


    [StringLength(50)]
    public string? ReceiptNo { get; set; }


    public bool HasReceipt { get; set; } = false;


    [StringLength(250)]
    public string? NoReceiptReason { get; set; }


    [Required]
    [StringLength(30)]
    public string FuelType { get; set; } = "GASOLINE";


    [Range(
        typeof(decimal),
        "0.0001",
        "999999999999")]
    public decimal Liters { get; set; }


    [Range(
        typeof(decimal),
        "0.0001",
        "999999999999")]
    public decimal UnitPrice { get; set; }


    [Required]
    [RegularExpression("^(NV|NV1|NV2|RV1|RV2)$")]
    public string TaxSchemeCode { get; set; } = "NV";


    [StringLength(500)]
    public string? Remarks { get; set; }
}


/* ================================================================
   DETAIL READ
   ================================================================ */

public sealed class FuelAcceptanceDetailDto
    : FuelAcceptanceDetailSaveDto
{
    public long FuelLineNo { get; set; }

    public string FuelAcceptanceNo { get; set; } = string.Empty;

    public decimal GrossAmount { get; set; }

    public decimal VatRate { get; set; }

    public decimal EwtRate { get; set; }

    public decimal BasicAmount { get; set; }

    public decimal InputVat { get; set; }

    public decimal EwtAmount { get; set; }

    public decimal NetAmount { get; set; }

    public string UserId { get; set; } = string.Empty;
}


/* ================================================================
   SAVE REQUEST
   ================================================================ */

public sealed class FuelAcceptanceSaveDto
{
    [Required]
    public FuelAcceptanceSaveHeaderDto Header { get; set; } = new();

    [Required]
    [MinLength(1)]
    public List<FuelAcceptanceDetailSaveDto> Details { get; set; } = [];
}


/* ================================================================
   DOCUMENT
   ================================================================ */

public sealed class FuelAcceptanceDocumentDto
{
    public FuelAcceptanceHeaderDto Header { get; set; } = new();

    public List<FuelAcceptanceDetailDto> Details { get; set; } = [];
}


/* ================================================================
   WORKFLOW
   ================================================================ */

public sealed class FuelAcceptanceWorkflowDto
{
    [Required]
    public string RowVersion { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Reason { get; set; }
}


/* ================================================================
   COMMAND RESPONSE
   ================================================================ */

public sealed class FuelAcceptanceCommandResultDto
{
    public string FuelAcceptanceNo { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string RowVersion { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}


/* ================================================================
   LEGACY PO LOOKUP

   Can be removed later after Angular migration is complete.
   ================================================================ */

public sealed class FuelPoLookupDto
{
    public string PoNo { get; set; } = string.Empty;

    public DateTime PoDate { get; set; }

    public string SuppCode { get; set; } = string.Empty;

    public string SuppName { get; set; } = string.Empty;

    public string BranchName { get; set; } = string.Empty;

    public string VehicleNo { get; set; } = string.Empty;

    public decimal PoAmount { get; set; }
}


/* ================================================================
   NEW SUPPLIER-BASED OPEN PO LINE LOOKUP
   ================================================================ */

public sealed class FuelPoLineLookupDto
{
    public string PoNo { get; set; } = string.Empty;

    public int PoLineNo { get; set; }

    public DateTime PoDate { get; set; }

    public string SuppCode { get; set; } = string.Empty;

    public string SuppName { get; set; } = string.Empty;

    public string ItemCode { get; set; } = string.Empty;

    public string ItemDesc { get; set; } = string.Empty;

    public decimal QtyPo { get; set; }

    public decimal QtyRcv { get; set; }

    /*
     * Quantity already used by other Draft/Posted
     * Fuel Acceptances.
     */
    public decimal FuelUsedQty { get; set; }

    public decimal OpenQty { get; set; }

    public string? PoUm { get; set; }

    public string? BaseUm { get; set; }

    public decimal? PoConvFactor { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineAmount { get; set; }

    public string FuelType { get; set; } = string.Empty;
}


/* ================================================================
   SUPPLIER LOOKUP
   ================================================================ */

public sealed class FuelSupplierLookupDto
{
    public string SuppCode { get; set; } = string.Empty;

    public string SuppName { get; set; } = string.Empty;
}


/* ================================================================
   BRANCH LOOKUP
   ================================================================ */

public sealed class FuelBranchLookupDto
{
    public string BranchName { get; set; } = string.Empty;
}


/* ================================================================
   TAX LOOKUP
   ================================================================ */

public sealed class FuelTaxSchemeLookupDto
{
    public string TaxSchemeCode { get; set; } = string.Empty;

    public string TaxName { get; set; } = string.Empty;

    public decimal VatRate { get; set; }

    public decimal EwtRate { get; set; }
}


/* ================================================================
   VP SOURCE

   PoNo may contain a display summary such as:
       066239, 066216, 065944
   ================================================================ */

public sealed class FuelVpSourceDto
{
    public string SourceType { get; set; } = "GAS";

    public string SourceNo { get; set; } = string.Empty;

    public string RefId { get; set; } = string.Empty;

    public string FuelAcceptanceNo { get; set; } = string.Empty;

    public DateTime AcceptanceDate { get; set; }

    public string? PoNo { get; set; }

    public string SuppCode { get; set; } = string.Empty;

    public string SuppName { get; set; } = string.Empty;

    public string BranchName { get; set; } = string.Empty;

    public decimal GrossAmount { get; set; }

    public decimal BasicAmount { get; set; }

    public decimal InputVat { get; set; }

    public decimal EwtAmount { get; set; }

    public decimal NetAmount { get; set; }
}


/* ================================================================
   PAGED RESULT
   ================================================================ */

public sealed class FuelPagedResultDto<T>
{
    public IReadOnlyList<T> Data { get; set; } = [];

    public int Total { get; set; }
}