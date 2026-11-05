
using System.Collections.Generic;

namespace HRIStwinbeeAPI.DTO.Quotation
{
    // ==========================================================
    // UNIFIED QUOTATION UPSERT REQUEST
    // ==========================================================

    public sealed class QuotationUpsertRequest
    {
        public string Action { get; set; } = "INSERT";

        public string SaveScope { get; set; } = "ALL";

        public string SqNo { get; set; } = "";

        public int? CustomerId { get; set; }

        public string? ProcessId { get; set; }

        public string? VatType { get; set; }

        public string? TermId { get; set; }

        public int? FinId { get; set; }

        public string? Warranty { get; set; }

        public string? Delivery { get; set; }

        public DateTime? SqDate { get; set; }

        public string? AcPerson { get; set; }

        public string? ContactP { get; set; }

        public string? Remarks { get; set; }

        public int? ArtCode { get; set; }

        // Header Discount:
        // 0.01 = 1%
        // 0.02 = 2%
        // 0.10 = 10%
        public decimal? Discnt { get; set; }

        public List<QuotationUpsertDetailDto> Details
        { get; set; } = new();
    }

    // ==========================================================
    // QUOTATION DETAIL DTO
    // ==========================================================

    public sealed class QuotationUpsertDetailDto
    {
        public int SeqLineNo { get; set; }

        public string Description { get; set; } = "";

        public string? Size { get; set; }

        public string? UM { get; set; }

        public decimal Qty { get; set; }

        public decimal UnitCost { get; set; }

        public decimal TotAmt { get; set; }

        // Detail discount amount, not Header Discount %
        public decimal Discnt { get; set; }

        public string? Materials { get; set; }

        public decimal VAT { get; set; }

        public string? ServiceId { get; set; }

        public string? FinalFin { get; set; }
    }

    // ==========================================================
    // QUOTATION UPSERT RESULT
    // ==========================================================

    public sealed class QuotationUpsertResultDto
    {
        public bool Success { get; set; }

        public string SqNo { get; set; } = "";

        public string Operation { get; set; } = "";

        public string SaveScope { get; set; } = "";

        public int DetailsUpdated { get; set; }

        public int DetailsInserted { get; set; }

        public string Message { get; set; } = "";
    }
}
