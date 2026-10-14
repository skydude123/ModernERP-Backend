using System.ComponentModel.DataAnnotations;

namespace HRIStwinbeeAPI.DTO.Receiving;

public sealed class ReceivingDocumentDto
{
    public string RrNo { get; set; } = string.Empty;
    public DateTime RrDate { get; set; }
    public string Branchname { get; set; } = string.Empty;
    public string WhseCode { get; set; } = string.Empty;
    public string WhseDesc { get; set; } = string.Empty;
    public int FiscalYear { get; set; }
    public string FiscalPeriod { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string? SuppCode { get; set; }
    public string? SuppName { get; set; }
    //public string? SoNo { get; set; }
    //public int? CustId { get; set; }
    //public string? CustName { get; set; }
    public string Status { get; set; } = "N";
    public string TransType { get; set; } = "RPO";
    public string? UserId { get; set; }
    public string? Remarks { get; set; }
    public DateTime? DateChanged { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public long NoOfReversal { get; set; }
    public string? ReversedBy { get; set; }
    public DateTime? ReversedWhen { get; set; }
    public string? ReversalRemarks { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public List<ReceivingDetailDto> Details { get; set; } = [];
}

public sealed class ReceivingDetailDto
{
    public int RrLineNo { get; set; }
    public string RrNo { get; set; } = string.Empty;
    public int RrSeq { get; set; }
    public int? PoLineNo { get; set; }
    public string? PoNo { get; set; }
    public string? ItemCode { get; set; }
    public string ItemDesc { get; set; } = string.Empty;
    public decimal QtyRcv { get; set; }
    public string BaseUm { get; set; } = string.Empty;
    public string? PoUm { get; set; }
    public decimal PoConvFactor { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineAmount { get; set; }
    public decimal BaseQty { get; set; }
    public decimal? OrderedQty { get; set; }
    public decimal? CumulativeReceivedQty { get; set; }
    public decimal? OpenQty { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime DateChanged { get; set; }
}

public sealed class UpsertReceivingRequest
{
    [StringLength(10)]
    public string? RrNo { get; set; }

    [Required]
    public DateTime RrDate { get; set; }

    [Required, StringLength(50)]
    public string Branchname { get; set; } = "MANDAUE";

    [Required, StringLength(5)]
    public string WhseCode { get; set; } = "WHSE";

    [Required, StringLength(20)]
    public string Reference { get; set; } = string.Empty;

    [StringLength(8)]
    public string? SuppCode { get; set; }

    //[StringLength(10)]
    //public string? SoNo { get; set; }

    //public int? CustId { get; set; }

    [Required, RegularExpression("^[NU]$")]
    public string TargetStatus { get; set; } = "N";

    [Required, RegularExpression("^(RPO|RWP)$")]
    public string TransType { get; set; } = "RPO";

    // Transitional fallback while API authentication is disabled.
    [StringLength(10)]
    public string? UserId { get; set; }

    [StringLength(100)]
    public string? Remarks { get; set; }

    public string? ExpectedRowVersion { get; set; }

    [Required, MinLength(1)]
    public List<ReceivingDetailUpsertDto> Details { get; set; } = [];
}

public sealed class ReceivingDetailUpsertDto
{
    [Range(1, int.MaxValue)]
    public int RrSeq { get; set; }

    public int? PoLineNo { get; set; }

    [StringLength(10)]
    public string? PoNo { get; set; }

    [StringLength(20)]
    public string? ItemCode { get; set; }

    [Required, StringLength(60)]
    public string ItemDesc { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.0001", "99999999999999")]
    public decimal QtyRcv { get; set; }

    [Required, StringLength(3)]
    public string BaseUm { get; set; } = string.Empty;

    [StringLength(3)]
    public string? PoUm { get; set; }

    [Range(typeof(decimal), "0.0001", "99999999999999")]
    public decimal PoConvFactor { get; set; } = 1;

    [Range(typeof(decimal), "0", "99999999999999")]
    public decimal UnitPrice { get; set; }
}

public sealed class ReverseReceivingRequest
{
    [StringLength(10)]
    public string? UserId { get; set; }

    [Required, StringLength(500, MinimumLength = 5)]
    public string Reason { get; set; } = string.Empty;

    [Required]
    public string ExpectedRowVersion { get; set; } = string.Empty;
}

public sealed class ReceivingSaveResultDto
{
    public string RrNo { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public byte[] RowVersion { get; set; } = [];
    public string Message { get; set; } = string.Empty;
}

public sealed class ReceivingQueryRequest
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Status { get; set; }
    public string? TransType { get; set; }
    public string? Branchname { get; set; }
    public string? WhseCode { get; set; }
    public string? SuppCode { get; set; }
    public string? Search { get; set; }
    public string UserId { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int Skip { get; set; }

    [Range(1, 500)]
    public int Take { get; set; } = 50;

    public string SortField { get; set; } = "rrDate";
    public string SortDir { get; set; } = "desc";
}

public sealed class ReceivingListItemDto
{
    public string RrNo { get; set; } = string.Empty;
    public DateTime RrDate { get; set; }
    public string Branchname { get; set; } = string.Empty;
    public string WhseCode { get; set; } = string.Empty;
    public string WhseDesc { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string? SuppCode { get; set; }
    public string? SuppName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string TransType { get; set; } = string.Empty;
    public string? PoNo { get; set; }
    public string? UserId { get; set; }
    public string? Remarks { get; set; }
    public long NoOfLines { get; set; }
    public decimal TotalQty { get; set; }
    public decimal TotalAmount { get; set; }
    public long NoOfReversal { get; set; }
    public string? ReversedBy { get; set; }
    public DateTime? ReversedWhen { get; set; }
}

public sealed class OpenPoQueryRequest
{
    public string? SuppCode { get; set; }
    public string? Search { get; set; }
    public string? WhseCode { get; set; }

    [Range(0, int.MaxValue)]
    public int Skip { get; set; }

    [Range(1, 200)]
    public int Take { get; set; } = 50;
}

public sealed class OpenPoLineDto
{
    public int PoLineNo { get; set; }
    public string PoNo { get; set; } = string.Empty;
    public DateTime PoDate { get; set; }
    public string SuppCode { get; set; } = string.Empty;
    public string? SuppName { get; set; }
    public string? ItemCode { get; set; }
    public string ItemDesc { get; set; } = string.Empty;
    public decimal QtyPo { get; set; }
    public decimal QtyRcv { get; set; }
    public decimal OpenQty { get; set; }
    public string BaseUm { get; set; } = string.Empty;
    public string? PoUm { get; set; }
    public decimal PoConvFactor { get; set; }
    public decimal UnitPrice { get; set; }
}

public sealed class SupplierLookupDto
{
    public string SuppCode { get; set; } = string.Empty;
    public string SuppName { get; set; } = string.Empty;
}

public sealed class BranchLookupDto
{
    public string Branchname { get; set; } = string.Empty;
}

public sealed class WarehouseLookupDto
{
    public string WhseCode { get; set; } = string.Empty;
    public string WhseDesc { get; set; } = string.Empty;
}

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Data { get; init; } = [];
    public long Total { get; init; }
}

public sealed class ReceivingItemLookupDto
{
    public string ItemCode { get; set; } = string.Empty;
    public string ItemDesc { get; set; } = string.Empty;
    public string BaseUm { get; set; } = string.Empty;
    public string? PoUm { get; set; }
    public decimal PoConvFactor { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public string? WhseCode { get; set; }
}

//public sealed class SoVerificationDto
//{
//    public string SoNo { get; set; } = string.Empty;

//    public int CustomerId { get; set; }

//    public string CustName { get; set; } = string.Empty;
//}
