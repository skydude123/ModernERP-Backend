using HRIStwinbeeAPI.DTO.Inventory.ItemMaster;

namespace HRIStwinbeeAPI.Repo.Interface.ItemMaster
{
    public interface IItemMasterRepository
    {
        Task<ItemMasterPagedResultDto> GetListAsync(
            string? search,
            string? groupCode,
            string? itemType,
            string active,
            int skip,
            int take,
            string sortField,
            string sortDir
        );

        Task<ItemMasterUpsertResponseDto> UpsertAsync(
            ItemMasterUpsertRequestDto request
        );

        Task<IEnumerable<ItemGroupLookupDto>> GetItemGroupLookupAsync(
            bool includeAll = true
        );

        Task<IEnumerable<ItemUmLookupDto>> GetItemUmLookupAsync();
    }
}