
using System;

namespace HRIStwinbeeAPI.DTO.Quotation;

public sealed class QuotationConfirmRequestDto
{
    public string ConfirmRef { get; set; } = string.Empty;

    public DateTime? ConfirmDate { get; set; }
}

public sealed class QuotationConfirmResultDto
{
    public string SqNo { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string ConfirmRef { get; set; } = string.Empty;

    public DateTime ConfirmDate { get; set; }
}
