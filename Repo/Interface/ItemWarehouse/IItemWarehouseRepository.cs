using HRIStwinbeeAPI.DTO.Inventory.ItemWarehouse;

namespace HRIStwinbeeAPI.Repo.Interface.ItemWarehouse
{
    public interface IItemWarehouseRepository
    {
        Task<IEnumerable<ItemWarehouseBalanceDto>> GetByItemCodeAsync(
           string itemCode,
           string active = "Y"
       );
    }
}
