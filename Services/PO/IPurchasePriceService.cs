using HRIStwinbeeAPI.Models.PO;

namespace HRIStwinbeeAPI.Services.PO;

public interface IPurchasePriceService
{
    Task<IReadOnlyList<SupplierPriceListItem>> GetSupplierPriceListAsync(
     CancellationToken cancellationToken);

    //Task<IReadOnlyList<SupplierPriceListItem>> GetSupplierPriceListAsync(
    //string supplierCode,
    //CancellationToken cancellationToken);

    Task<IReadOnlyList<PurchasePriceItemUm>> GetItemUmsAsync(
        string itemCode,
        CancellationToken cancellationToken);

    Task<PurchasePriceResolution> ResolveAsync(
        string supplierCode,
        string itemCode,
        string poUm,
        DateTime? asOfDate,
        CancellationToken cancellationToken);

    Task<PurchasePricePagedResult<SupplierPriceRecord>> GetCurrentPageAsync(
        PurchasePriceCurrentQuery query,
        CancellationToken cancellationToken);

    Task<PurchasePriceInquiryResult> GetInquiryAsync(
        string supplierCode,
        string itemCode,
        string poUm,
        DateTime? asOfDate,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<SupplierPriceUpsertResult> UpsertAsync(
        SupplierPriceUpsertRequest request,
        CancellationToken cancellationToken);

    Task<PurchasePricePagedResult<SupplierPriceHistoryRecord>> GetMasterHistoryPageAsync(
        PurchasePriceHistoryQuery query,
        CancellationToken cancellationToken);

    Task<PurchasePricePagedResult<ActualPoPriceHistoryRecord>> GetActualPoHistoryPageAsync(
        PurchasePriceHistoryQuery query,
        CancellationToken cancellationToken);
}
