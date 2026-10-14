namespace HRIStwinbeeAPI.DTO.Inventory.ItemMaster
{
    public class ItemMasterUpsertResponseDto
    {
        public bool Success { get; set; }

        public string? Message { get; set; }

        public ItemMasterListDto? Item { get; set; }
    }


    public class ItemMasterCommandRowDto
    {
        public bool Success { get; set; }

        public string? Message { get; set; }

        public string? ItemCode { get; set; }

        public string? ItemDesc { get; set; }

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