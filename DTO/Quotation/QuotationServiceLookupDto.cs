namespace HRIStwinbeeAPI.DTO.Quotation
{
    public class QuotationServiceLookupDto
    {
        public string ServiceId { get; set; } = string.Empty;

        public string ServiceName { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        public bool IsActive { get; set; }
    }
}
