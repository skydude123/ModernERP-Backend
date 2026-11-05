namespace HRIStwinbeeAPI.DTO.Quotation
{
    public sealed class QuotationListDto
    {
        public string? SqNo { get; set; }
        public string? CustName { get; set; }
        public DateTime? SqDate { get; set; }
        public string? Status { get; set; }
        public string? ContactP { get; set; }
        public string? AcPerson { get; set; }
        public string? BranchName { get; set; }
        public string? UserId { get; set; }
        public string? VatType { get; set; }
        public string? ProcessName { get; set; }
        public string? ConfirmRef { get; set; }
        public DateTime? ConfirmDate { get; set; }
        public string? SoNo { get; set; }
        public string? DrNo { get; set; }
        public string? Flag { get; set; }
    }
}
