using HRIStwinbeeAPI.DTO.StockItemInquiry;

namespace HRIStwinbeeAPI.Repo.Interface.StockItemInquiry;

public interface IStockItemInquiryRepository
{
    Task<StockItemPagedResultDto<StockItemDto>> GetItemsAsync(
        StockItemListQueryDto query,
        CancellationToken token);

    Task<IReadOnlyList<WarehouseStockDto>> GetWarehouseStocksAsync(
        string itemCode,
        CancellationToken token);

    Task<IReadOnlyList<WarehouseLotDto>> GetWarehouseLotsAsync(
        string itemCode,
        string whseCode,
        bool openOnly,
        CancellationToken token);

    Task<IReadOnlyList<StockCardDto>> GetStockCardAsync(
        string itemCode,
        string whseCode,
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken token);
}
