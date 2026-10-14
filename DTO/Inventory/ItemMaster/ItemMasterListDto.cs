namespace HRIStwinbeeAPI.DTO.Inventory.ItemMaster
{
    public class ItemMasterListDto
    {
        public string? ItemCode { get; set; }
        public string? ItemDesc { get; set; }

        // IMPORTANT:
        // Needed for edit/upsert.
        public string? GroupCode { get; set; }

        public string? GroupDesc { get; set; }
        public string? ItemType { get; set; }

        public string? Um { get; set; }
        public decimal? BufferQty { get; set; }
        public decimal? UnitCost { get; set; }
        public decimal? LastUnitCost { get; set; }

        public string? Active { get; set; }
    }
}
