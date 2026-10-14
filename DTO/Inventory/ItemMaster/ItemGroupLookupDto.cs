namespace HRIStwinbeeAPI.DTO.Inventory.ItemMaster
{
    public class ItemGroupLookupDto
    {
        public string GroupCode { get; set; } = string.Empty;
        public string GroupDesc { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }
}
