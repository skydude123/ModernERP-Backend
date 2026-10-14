using HRIStwinbeeAPI.DTO.StockTransfer;

namespace HRIStwinbeeAPI.Repo.Interface.StockTransfer;

public interface IStockTransferRepository
{
    Task<StockTransferPagedResult<StockTransferListItemDto>> QueryAsync(
        StockTransferQueryRequest request,
        CancellationToken token);

    Task<StockTransferDocumentDto?> GetByNoAsync(string stNo, CancellationToken token);

    Task<IReadOnlyList<StockTransferWarehouseDto>> GetWarehousesAsync(
        string? search,
        CancellationToken token);

    Task<IReadOnlyList<StockTransferSourceItemDto>> GetSourceItemsAsync(
        string sourceWhseCode,
        string? search,
        CancellationToken token);

    Task<StockTransferSaveResultDto> UpsertAsync(
        UpsertStockTransferRequest request,
        CancellationToken token);

    Task<StockTransferReverseResultDto> ReverseAsync(
     ReverseStockTransferRequest request,
     CancellationToken token);
}
