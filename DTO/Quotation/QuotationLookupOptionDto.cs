namespace HRIStwinbeeAPI.DTO.Quotation
{
    public sealed class QuotationLookupOptionDto
    {
        public string Code { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int SortOrder { get; set; }
    }
}
