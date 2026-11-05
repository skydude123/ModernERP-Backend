namespace HRIStwinbeeAPI.DTO.Quotation;

public sealed class QuotationHeaderDto
{
    public string? SqNo { get; set; }
    public string? CustomerId { get; set; }
    public string? CustName { get; set; }

    public string? ProcessId { get; set; }
    public string? VatType { get; set; }

    public string? TermId { get; set; }
    public string? TermName { get; set; }
    public string? ProcessName { get; set; }

    public string? FinId { get; set; }
    public string? FinName { get; set; }

    public string? Warranty { get; set; }
    public string? Delivery { get; set; }

    // Header percentage fraction
    // 0.01 = 1%
    public decimal? Discnt { get; set; }

    public DateTime? SqDate { get; set; }
    public string? Status { get; set; }

    public string? AcPerson { get; set; }
    public string? ContactP { get; set; }
    public string? Remarks { get; set; }

    public string? BranchName { get; set; }
    public string? UserId { get; set; }
    public DateTime? TrnDate { get; set; }

    public string? ArtCode { get; set; }
    public string? ArtistName { get; set; }
}

public sealed class QuotationDetailDto
{
    public int SeqLineNo { get; set; }
    public string? SqNo { get; set; }
    public string? Desc { get; set; }
    public string? Size { get; set; }
    public string? ServiceId { get; set; }
    public string? Um { get; set; }

    public decimal Qty { get; set; }
    public decimal UnitCost { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discnt { get; set; }

    public string? Materials { get; set; }
    public string? FinalFin { get; set; }

    public decimal Total { get; set; }
    public decimal Vat { get; set; }
}

public sealed class QuotationAdditionalChargeDto
{
    public string? Chgn1 { get; set; }
    public decimal? ChgVal1 { get; set; }

    public string? Chgn2 { get; set; }
    public decimal? ChgVal2 { get; set; }

    public string? Chgn3 { get; set; }
    public decimal? ChgVal3 { get; set; }

    public decimal? Discnt { get; set; }
}

public sealed class QuotationTermDto
{
    public string? TermId { get; set; }
    public string? TermName { get; set; }
}

public sealed class QuotationArtistDto
{
    public string? ArtCode { get; set; }
    public string? ArtistName { get; set; }
}

public sealed class QuotationCustomerDto
{
    public string? CustId { get; set; }
    public string? CustName { get; set; }
    public string? CPerson { get; set; }
    public string? CPosition { get; set; }
    public string? CContact { get; set; }
    public string? EmailAdd { get; set; }
}

public sealed class QuotationReadDto
{
    public QuotationHeaderDto Header { get; set; } = new();

    public IReadOnlyList<QuotationDetailDto> Details
    { get; set; } = Array.Empty<QuotationDetailDto>();

    public QuotationAdditionalChargeDto? AdditionalCharges
    { get; set; }
}
