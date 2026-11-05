namespace HRIStwinbeeAPI.DTO.Quotation
{
    public sealed class QuotationCreateRequest
    {
        public int CustomerId { get; set; }

        public DateTime SqDate { get; set; }

        public string? VatType { get; set; }

        public string? TermId { get; set; }

        public string? AcPerson { get; set; }

        public string? ContactP { get; set; }

        public string? Remarks { get; set; }

        public int? ArtCode { get; set; }

        public string? Delivery { get; set; }

        public string? Warranty { get; set; }

        public List<QuotationCreateDetailDto> Details { get; set; } = new();
    }

    public sealed class QuotationCreateDetailDto
    {
        public int SeqLineNo { get; set; }

        public string Description { get; set; } = string.Empty;

        public string? Size { get; set; }

        public string? UM { get; set; }

        public decimal Qty { get; set; }

        public decimal UnitCost { get; set; }

        public decimal TotAmt { get; set; }

        public decimal Discnt { get; set; }

        public string? Materials { get; set; }

        public decimal VAT { get; set; }

        public string? ServiceId { get; set; }

        public string? FinalFin { get; set; }
    }
}
