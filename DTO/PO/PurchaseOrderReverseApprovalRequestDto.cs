namespace HRIStwinbeeAPI.DTO.PO
{
    public class PurchaseOrderReverseApprovalRequestDto
    {
        public string PoNo { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? Remarks { get; set; }
    }
}
