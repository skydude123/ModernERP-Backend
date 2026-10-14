using HRIStwinbeeAPI.DTO.StockItemInquiry;
using HRIStwinbeeAPI.Repo.Interface.StockItemInquiry;

namespace HRIStwinbeeAPI.Services.StockItemInquiry;

public sealed class StockItemInquiryService(IStockItemInquiryRepository repository)
    : IStockItemInquiryService
{
    public Task<StockItemPagedResultDto<StockItemDto>> GetItemsAsync(
        StockItemListQueryDto query,
        CancellationToken token)
    {
        query.Search = Clean(query.Search, 100, "Search");
        query.GroupCode = Clean(query.GroupCode, 10, "Group code");
        if (query.Skip < 0) throw new ArgumentOutOfRangeException(nameof(query.Skip), "Skip cannot be negative.");
        if (query.Take is < 1 or > 200) throw new ArgumentOutOfRangeException(nameof(query.Take), "Take must be between 1 and 200.");
        return repository.GetItemsAsync(query, token);
    }

    public Task<IReadOnlyList<WarehouseStockDto>> GetWarehouseStocksAsync(
        string itemCode,
        CancellationToken token) =>
        repository.GetWarehouseStocksAsync(Required(itemCode, 20, "Item code"), token);

    public Task<IReadOnlyList<WarehouseLotDto>> GetWarehouseLotsAsync(
        string itemCode,
        string whseCode,
        bool openOnly,
        CancellationToken token) =>
        repository.GetWarehouseLotsAsync(
            Required(itemCode, 20, "Item code"),
            Required(whseCode, 5, "Warehouse code"),
            openOnly,
            token);

    public Task<IReadOnlyList<StockCardDto>> GetStockCardAsync(
        string itemCode,
        string whseCode,
        StockCardQueryDto query,
        CancellationToken token)
    {
        var today = DateTime.Today;
        var dateFrom = query.DateFrom?.Date ?? new DateTime(today.Year, 1, 1);
        var dateTo = query.DateTo?.Date ?? today;
        if (dateFrom > dateTo) throw new ArgumentException("Date From cannot be later than Date To.");

        return repository.GetStockCardAsync(
            Required(itemCode, 20, "Item code"),
            Required(whseCode, 5, "Warehouse code"),
            dateFrom,
            dateTo,
            token);
    }

    private static string Required(string? value, int maxLength, string label) =>
        Clean(value, maxLength, label) ?? throw new ArgumentException($"{label} is required.");

    private static string? Clean(string? value, int maxLength, string label)
    {
        var clean = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        if (clean?.Length > maxLength) throw new ArgumentException($"{label} cannot exceed {maxLength} characters.");
        return clean;
    }
}
