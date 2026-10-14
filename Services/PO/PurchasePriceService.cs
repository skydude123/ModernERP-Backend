using HRIStwinbeeAPI.Models.PO;
using HRIStwinbeeAPI.Repo.Interface.PO;
using HRIStwinbeeAPI.Services.Suppliers;
using Microsoft.AspNetCore.Connections;

namespace HRIStwinbeeAPI.Services.PO;

public sealed class PurchasePriceService(
    IPurchasePriceRepository repository,
    ICurrentUserContext currentUserContext) : IPurchasePriceService
{
    public async Task<IReadOnlyList<SupplierPriceListItem>> GetSupplierPriceListAsync(
     CancellationToken cancellationToken)
    {
        return await repository.GetSupplierPriceListAsync(
            cancellationToken);
    }

    //public async Task<IReadOnlyList<SupplierPriceListItem>> GetSupplierPriceListAsync(
    // string supplierCode,
    // CancellationToken cancellationToken)
    //{
    //    var rows =
    //        await repository.GetSupplierPriceListAsync(
    //            Required(
    //                supplierCode,
    //                PurchasePriceFieldLengths.SupplierCode,
    //                "Supplier code"
    //            ),
    //            cancellationToken
    //        );

    //    if (rows.Count > 5000)
    //    {
    //        throw new PurchasePriceValidationException(
    //            "This supplier has more than 5,000 price records. Use a server-filtered price search instead."
    //        );
    //    }

    //    return rows;
    //}

    public Task<IReadOnlyList<PurchasePriceItemUm>> GetItemUmsAsync(
        string itemCode,
        CancellationToken cancellationToken) =>
        repository.GetItemUmsAsync(
            Required(itemCode, PurchasePriceFieldLengths.ItemCode, "Item code"),
            cancellationToken);

    public Task<PurchasePriceResolution> ResolveAsync(
        string supplierCode,
        string itemCode,
        string poUm,
        DateTime? asOfDate,
        CancellationToken cancellationToken) =>
        repository.ResolveAsync(
            Required(supplierCode, PurchasePriceFieldLengths.SupplierCode, "Supplier code"),
            Required(itemCode, PurchasePriceFieldLengths.ItemCode, "Item code"),
            Required(poUm, PurchasePriceFieldLengths.PoUm, "PO U/M"),
            asOfDate?.Date,
            cancellationToken);

    public Task<PurchasePricePagedResult<SupplierPriceRecord>> GetCurrentPageAsync(
        PurchasePriceCurrentQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        query.SupplierCode = OptionalCode(query.SupplierCode, PurchasePriceFieldLengths.SupplierCode, "Supplier code");
        query.ItemCode = OptionalCode(query.ItemCode, PurchasePriceFieldLengths.ItemCode, "Item code");
        query.PoUm = OptionalCode(query.PoUm, PurchasePriceFieldLengths.PoUm, "PO U/M");
        query.Active = string.IsNullOrWhiteSpace(query.Active)
            ? "ALL"
            : query.Active.Trim().ToUpperInvariant();

        if (query.Active is not ("ALL" or "Y" or "N"))
        {
            throw new PurchasePriceValidationException("Active filter must be ALL, Y, or N.");
        }

        NormalizePage(query);
        return repository.GetCurrentPageAsync(query, cancellationToken);
    }

    public Task<PurchasePriceInquiryResult> GetInquiryAsync(
        string supplierCode,
        string itemCode,
        string poUm,
        DateTime? asOfDate,
        int skip,
        int take,
        CancellationToken cancellationToken) =>
        repository.GetInquiryAsync(
            Required(supplierCode, PurchasePriceFieldLengths.SupplierCode, "Supplier code"),
            Required(itemCode, PurchasePriceFieldLengths.ItemCode, "Item code"),
            Required(poUm, PurchasePriceFieldLengths.PoUm, "PO U/M"),
            asOfDate?.Date,
            Math.Max(skip, 0),
            Math.Clamp(take, 1, 250),
            cancellationToken);

    public Task<SupplierPriceUpsertResult> UpsertAsync(
        SupplierPriceUpsertRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        request.SupplierCode = Required(request.SupplierCode, PurchasePriceFieldLengths.SupplierCode, "Supplier code");
        request.ItemCode = Required(request.ItemCode, PurchasePriceFieldLengths.ItemCode, "Item code");
        request.PoUm = Required(request.PoUm, PurchasePriceFieldLengths.PoUm, "PO U/M");
        request.Remarks = OptionalText(request.Remarks, PurchasePriceFieldLengths.Remarks, "Remarks");
        request.SourceReference = OptionalText(
            request.SourceReference,
            PurchasePriceFieldLengths.SourceReference,
            "Source reference");

        if (request.UnitPrice < 0)
        {
            throw new PurchasePriceValidationException("Unit price must be zero or greater.");
        }

        if (request.EffectiveDate < new DateTime(1753, 1, 1))
        {
            throw new PurchasePriceValidationException("Effective date is required.");
        }

        if (request.ExpectedRowVersion is { Length: not 8 })
        {
            throw new PurchasePriceValidationException("Expected row version is invalid.");
        }

        return repository.UpsertAsync(
            request,
            currentUserContext.GetRequiredUserId(),
            cancellationToken);
    }

    public Task<PurchasePricePagedResult<SupplierPriceHistoryRecord>> GetMasterHistoryPageAsync(
        PurchasePriceHistoryQuery query,
        CancellationToken cancellationToken)
    {
        NormalizeHistory(query);
        return repository.GetMasterHistoryPageAsync(query, cancellationToken);
    }

    public Task<PurchasePricePagedResult<ActualPoPriceHistoryRecord>> GetActualPoHistoryPageAsync(
        PurchasePriceHistoryQuery query,
        CancellationToken cancellationToken)
    {
        NormalizeHistory(query);
        return repository.GetActualPoHistoryPageAsync(query, cancellationToken);
    }

    private static void NormalizeHistory(PurchasePriceHistoryQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        query.SupplierCode = OptionalCode(query.SupplierCode, PurchasePriceFieldLengths.SupplierCode, "Supplier code");
        query.ItemCode = Required(query.ItemCode, PurchasePriceFieldLengths.ItemCode, "Item code");
        query.PoUm = Required(query.PoUm, PurchasePriceFieldLengths.PoUm, "PO U/M");
        query.FromDate = query.FromDate?.Date;
        query.ToDate = query.ToDate?.Date;

        if (query.FromDate.HasValue && query.ToDate.HasValue && query.FromDate > query.ToDate)
        {
            throw new PurchasePriceValidationException("From date cannot be later than to date.");
        }

        NormalizePage(query);
    }

    private static void NormalizePage(PurchasePriceCurrentQuery query)
    {
        query.Skip = Math.Max(query.Skip, 0);
        query.Take = Math.Clamp(query.Take, 1, 250);
    }

    private static void NormalizePage(PurchasePriceHistoryQuery query)
    {
        query.Skip = Math.Max(query.Skip, 0);
        query.Take = Math.Clamp(query.Take, 1, 250);
    }

    private static string Required(string? value, int length, string name) =>
        OptionalCode(value, length, name)
        ?? throw new PurchasePriceValidationException($"{name} is required.");

    private static string? OptionalCode(string? value, int length, string name) =>
        OptionalText(value, length, name)?.ToUpperInvariant();

    private static string? OptionalText(string? value, int length, string name)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrEmpty(normalized))
        {
            return null;
        }

        if (normalized.Length > length)
        {
            throw new PurchasePriceValidationException($"{name} cannot exceed {length} characters.");
        }

        return normalized;
    }
}
