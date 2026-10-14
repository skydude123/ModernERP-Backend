namespace HRIStwinbeeAPI.DTO.PO
{
    public sealed class SupplierLookupDto
    {
        public string SuppCode { get; set; } = string.Empty;
        public string SuppName { get; set; } = string.Empty;
    }

    public sealed class BranchLookupDto
    {
        public string BranchName { get; set; } = string.Empty;
    }

    public sealed class TermLookupDto
    {
        public string TermCode { get; set; } = string.Empty;
        public string TermDescription { get; set; } = string.Empty;
    }

    public sealed class VatTypeLookupDto
    {
        public string VatType { get; set; } = string.Empty;
        public string TaxName { get; set; } = string.Empty;
    }

    public sealed class SoVerificationDto
    {
        public string SoNo { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustName { get; set; } = string.Empty;
    }

    public sealed class ItemLookupDto
    {
        public string ItemCode { get; set; } = string.Empty;
        public string ItemDesc { get; set; } = string.Empty;
        public string UmCode { get; set; } = string.Empty;
        //public string DefaultUm { get; set; } = string.Empty;
    }

    public sealed class PurchaseOrderDetailSaveDto
    {
        public int RowNo { get; set; }
        public int? PoLineNo { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public decimal QtyPo { get; set; }
        public string PoUm { get; set; } = string.Empty;
        public decimal PoConvFactor { get; set; } = 1;
        public decimal UnitPrice { get; set; } = 1;
    }

    public sealed class PurchaseOrderSaveRequestDto
    {
        public string? PoNo { get; set; }
        public DateTime PoDate { get; set; }
        public DateTime? PoDelDate { get; set; }
        public string PoType { get; set; } = "WO";
        public string TermCode { get; set; } = string.Empty;
        public string Status { get; set; } = "N";
        public string VatType { get; set; } = string.Empty;
        public string? ChequeType { get; set; }
        public string SuppCode { get; set; } = string.Empty;
        public string BranchName { get; set; } = "MANDAUE";
        public string? SoNo { get; set; }
        public string? Remarks { get; set; }
        public int? CustId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public bool ReplaceDetails { get; set; } = true;
        public List<PurchaseOrderDetailSaveDto> Details { get; set; } = new();
    }

    public sealed class PurchaseOrderSaveHeaderResultDto
    {
        public string PoNo { get; set; } = string.Empty;
        public string ReturnPONumber { get; set; } = string.Empty;
        public string ResultMessage { get; set; } = string.Empty;
    }

    public sealed class PurchaseOrderDetailResultDto
    {
        public int PoLineNo { get; set; }
        public string PoNo { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string ItemDesc { get; set; } = string.Empty;
        public decimal QtyPo { get; set; }
        public string PoUm { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal PoConvFactor { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    public sealed class PurchaseOrderSaveResponseDto
    {
        public string PoNo { get; set; } = string.Empty;
        public string ReturnPONumber { get; set; } = string.Empty;
        public string ResultMessage { get; set; } = string.Empty;
        public List<PurchaseOrderDetailResultDto> Details { get; set; } = new();
    }

    public sealed class PurchaseOrderHeaderDto
    {
        public string PoNo { get; set; } = string.Empty;
        public DateTime PoDate { get; set; }
        public DateTime? PoDelDate { get; set; }
        public string PoType { get; set; } = "WO";
        public string TermCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string VatType { get; set; } = string.Empty;
        public string? ChequeType { get; set; }
        public string SuppCode { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string? SoNo { get; set; }
        public string? Remarks { get; set; }
        public int? CustId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? CustName { get; set; }
    }

    public sealed class PurchaseOrderGetResponseDto
    {
        public PurchaseOrderHeaderDto? Header { get; set; }
        public List<PurchaseOrderDetailResultDto> Details { get; set; } = new();
    }

    public sealed class PurchaseOrderListQueryDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Status { get; set; } = "ALL";
        public string? BranchName { get; set; }
        public string? Search { get; set; }
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 50;
        public string SortField { get; set; } = "poDate";
        public string SortDir { get; set; } = "desc";
        public string UserId { get; set; } = string.Empty;
    }

    public sealed class PurchaseOrderListItemDto
    {
        public string PoNo { get; set; } = string.Empty;
        public DateTime PoDate { get; set; }
        public DateTime? PoDelDate { get; set; }
        public string PoType { get; set; } = string.Empty;
        public string TermCode { get; set; } = string.Empty;
        public string TermDescription { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public string VatType { get; set; } = string.Empty;
        public string TaxName { get; set; } = string.Empty;
        public string ChequeType { get; set; } = string.Empty;
        public string ChequeTypeName { get; set; } = string.Empty;
        public string SuppCode { get; set; } = string.Empty;
        public string SuppName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string SoNo { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public int? CustId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime? DateChanged { get; set; }
        public int LineCount { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalCount { get; set; }
    }

    public sealed class PagedResultDto<T>
    {
        public IReadOnlyList<T> Data { get; set; } = Array.Empty<T>();
        public int Total { get; set; }
    }

    public sealed class PurchaseOrderStatusActionRequestDto
    {
        public string PoNo { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? Remarks { get; set; }
    }

    public sealed class PurchaseOrderStatusActionResultDto
    {
        public string PoNo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public string ApprovedBy { get; set; } = string.Empty;
        public DateTime? DateApproved { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime? DateChanged { get; set; }
        public string? Remarks { get; set; }
        public string ResultMessage { get; set; } = string.Empty;


    }


    public class UmDto {
        public string? umCode { get; set; }
        public string? umDesc { get; set; }
    }
}
