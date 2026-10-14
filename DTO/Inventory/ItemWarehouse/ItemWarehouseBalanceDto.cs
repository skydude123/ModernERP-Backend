namespace HRIStwinbeeAPI.DTO.Inventory.ItemWarehouse
{
    public class ItemWarehouseBalanceDto
    {
        public string? WarehouseDescription { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemDescription { get; set; }
        public string? BaseUm { get; set; }
        public decimal QtyReceived { get; set; }
        public decimal QtyIssued { get; set; }
        public decimal QtyForIssue { get; set; }
        public decimal OnHandCost { get; set; }
    }
}
