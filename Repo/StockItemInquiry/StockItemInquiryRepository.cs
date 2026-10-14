using System.Data;
using Dapper;
using HRIStwinbeeAPI.ConnectionData;
using HRIStwinbeeAPI.DTO.StockItemInquiry;
using HRIStwinbeeAPI.Repo.Interface.StockItemInquiry;

namespace HRIStwinbeeAPI.Repo.StockItemInquiry;

public sealed class StockItemInquiryRepository(ISqlConnectionFactory factory)
    : IStockItemInquiryRepository
{
    public async Task<StockItemPagedResultDto<StockItemDto>> GetItemsAsync(
        StockItemListQueryDto query,
        CancellationToken token)
    {
        await using var connection = await factory.CreateOpenConnectionAsync(cancellationToken: token);
        using var result = await connection.QueryMultipleAsync(new CommandDefinition(
            "dbo.usp_StockItemInquiry_Items",
            new
            {
                search = Clean(query.Search),
                group_code = Clean(query.GroupCode),
                skip = query.Skip,
                take = query.Take
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: token));

        var rows = (await result.ReadAsync<StockItemDto>()).AsList();
        var total = await result.ReadSingleAsync<long>();
        return new StockItemPagedResultDto<StockItemDto> { Data = rows, Total = total };
    }

    public Task<IReadOnlyList<WarehouseStockDto>> GetWarehouseStocksAsync(
        string itemCode,
        CancellationToken token) =>
        QueryListAsync<WarehouseStockDto>(
            "dbo.usp_StockItemInquiry_WarehouseStocks",
            new { item_code = itemCode },
            token);

    public Task<IReadOnlyList<WarehouseLotDto>> GetWarehouseLotsAsync(
        string itemCode,
        string whseCode,
        bool openOnly,
        CancellationToken token) =>
        QueryListAsync<WarehouseLotDto>(
            "dbo.usp_StockItemInquiry_WarehouseLots",
            new { item_code = itemCode, whse_code = whseCode, open_only = openOnly },
            token);

    public Task<IReadOnlyList<StockCardDto>> GetStockCardAsync(
        string itemCode,
        string whseCode,
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken token) =>
        QueryListAsync<StockCardDto>(
            "dbo.usp_StockItemInquiry_StockCard",
            new
            {
                item_code = itemCode,
                whse_code = whseCode,
                date_from = dateFrom.Date,
                date_to = dateTo.Date
            },
            token);

    private async Task<IReadOnlyList<T>> QueryListAsync<T>(
        string procedure,
        object parameters,
        CancellationToken token)
    {
        await using var connection = await factory.CreateOpenConnectionAsync(cancellationToken: token);
        return (await connection.QueryAsync<T>(new CommandDefinition(
            procedure,
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: token))).AsList();
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
