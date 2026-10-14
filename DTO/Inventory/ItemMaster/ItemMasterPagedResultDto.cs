namespace HRIStwinbeeAPI.DTO.Inventory.ItemMaster
{
    public sealed class ItemMasterPagedResultDto
    {
        public List<ItemMasterListDto> Data { get; set; } = new();

        public long Total { get; set; }

        public int Skip { get; set; }

        public int Take { get; set; }
    }
}
