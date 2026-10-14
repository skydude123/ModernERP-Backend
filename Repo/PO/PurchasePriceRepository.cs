using System.Data;
using Dapper;
using HRIStwinbeeAPI.ConnectionData;
using HRIStwinbeeAPI.Models.PO;
using HRIStwinbeeAPI.Repo.Interface.PO;
using Microsoft.Data.SqlClient;

namespace HRIStwinbeeAPI.Repo.PO;

public sealed class PurchasePriceRepository(
    ISqlConnectionFactory connectionFactory) : IPurchasePriceRepository
{

    public async Task<IReadOnlyList<SupplierPriceListItem>> GetSupplierPriceListAsync(
     CancellationToken cancellationToken)
    {
        await using var connection =
            await connectionFactory.CreateOpenConnectionAsync(
                cancellationToken: cancellationToken);

        return (
            await connection.QueryAsync<SupplierPriceListItem>(
                Command(
                    "dbo.usp_PurchasePrice_SupplierList",
                    null,
                    cancellationToken
                )
            )
        ).AsList();
    }
    //public async Task<IReadOnlyList<SupplierPriceListItem>> GetSupplierPriceListAsync(
    //    string supplierCode,
    //    CancellationToken cancellationToken)
    //{
    //    await using var connection = await connectionFactory.CreateOpenConnectionAsync(
    //        cancellationToken: cancellationToken);

    //    var parameters = new DynamicParameters();
    //    parameters.Add("supplier_code", supplierCode, DbType.AnsiString, size: 10);

    //    return (await connection.QueryAsync<SupplierPriceListItem>(
    //        Command("dbo.usp_PurchasePrice_SupplierList", parameters, cancellationToken)))
    //        .AsList();
    //}

    public async Task<IReadOnlyList<PurchasePriceItemUm>> GetItemUmsAsync(
        string itemCode,
        CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(
            cancellationToken: cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("item_code", itemCode, DbType.AnsiString, size: 20);

        return (await connection.QueryAsync<PurchasePriceItemUm>(
            Command("dbo.usp_PurchasePrice_ItemUms", parameters, cancellationToken)))
            .AsList();
    }

    public async Task<PurchasePriceResolution> ResolveAsync(
        string supplierCode,
        string itemCode,
        string poUm,
        DateTime? asOfDate,
        CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(
            cancellationToken: cancellationToken);

        return await connection.QuerySingleAsync<PurchasePriceResolution>(
            Command(
                "dbo.usp_PurchasePrice_Resolve",
                ResolutionParameters(supplierCode, itemCode, poUm, asOfDate),
                cancellationToken));
    }

    public async Task<PurchasePricePagedResult<SupplierPriceRecord>> GetCurrentPageAsync(
        PurchasePriceCurrentQuery query,
        CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(
            cancellationToken: cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("supplier_code", query.SupplierCode, DbType.AnsiString, size: 10);
        parameters.Add("item_code", query.ItemCode, DbType.AnsiString, size: 20);
        parameters.Add("po_um", query.PoUm, DbType.AnsiString, size: 3);
        parameters.Add("active", query.Active, DbType.AnsiString, size: 3);
        parameters.Add("as_of_date", query.AsOfDate?.Date, DbType.Date);
        parameters.Add("skip", query.Skip, DbType.Int32);
        parameters.Add("take", query.Take, DbType.Int32);

        using var results = await connection.QueryMultipleAsync(
            Command("dbo.usp_PurchasePrice_CurrentPage", parameters, cancellationToken));

        var data = (await results.ReadAsync<SupplierPriceRecord>()).AsList();
        var total = await results.ReadSingleAsync<long>();

        return new PurchasePricePagedResult<SupplierPriceRecord>
        {
            Data = data,
            Total = total
        };
    }

    public async Task<PurchasePriceInquiryResult> GetInquiryAsync(
        string supplierCode,
        string itemCode,
        string poUm,
        DateTime? asOfDate,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(
            cancellationToken: cancellationToken);

        var parameters = ResolutionParameters(supplierCode, itemCode, poUm, asOfDate);
        parameters.Add("skip", skip, DbType.Int32);
        parameters.Add("take", take, DbType.Int32);

        using var results = await connection.QueryMultipleAsync(
            Command("dbo.usp_PurchasePrice_Inquiry", parameters, cancellationToken));

        var resolution = await results.ReadSingleAsync<PurchasePriceResolution>();
        var prices = (await results.ReadAsync<SupplierPriceComparison>()).AsList();
        var aggregate = await results.ReadSingleAsync<PurchasePriceInquiryAggregate>();

        return new PurchasePriceInquiryResult
        {
            Resolution = resolution,
            ConfiguredPrices = prices,
            Total = aggregate.Total,
            LowestConfiguredPrice = aggregate.LowestConfiguredPrice,
            HighestConfiguredPrice = aggregate.HighestConfiguredPrice
        };
    }

    public async Task<SupplierPriceUpsertResult> UpsertAsync(
        SupplierPriceUpsertRequest request,
        string userId,
        CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(
            cancellationToken: cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("supplier_code", request.SupplierCode, DbType.AnsiString, size: 10);
        parameters.Add("item_code", request.ItemCode, DbType.AnsiString, size: 20);
        parameters.Add("po_um", request.PoUm, DbType.AnsiString, size: 3);
        parameters.Add("unit_price", request.UnitPrice, DbType.Decimal, precision: 18, scale: 4);
        parameters.Add("effective_date", request.EffectiveDate.Date, DbType.Date);
        parameters.Add("is_active", request.IsActive, DbType.Boolean);
        parameters.Add("remarks", request.Remarks, DbType.String, size: 500);
        parameters.Add("source_reference", request.SourceReference, DbType.AnsiString, size: 50);
        parameters.Add("expected_row_version", request.ExpectedRowVersion, DbType.Binary, size: 8);
        parameters.Add("user_id", userId, DbType.AnsiString, size: 10);

        try
        {
            return await connection.QuerySingleAsync<SupplierPriceUpsertResult>(
                Command("dbo.usp_PurchasePrice_Upsert", parameters, cancellationToken));
        }
        catch (SqlException exception) when (exception.Number >= 50000)
        {
            throw Translate(exception);
        }
    }

    public Task<PurchasePricePagedResult<SupplierPriceHistoryRecord>> GetMasterHistoryPageAsync(
        PurchasePriceHistoryQuery query,
        CancellationToken cancellationToken) =>
        QueryHistoryAsync<SupplierPriceHistoryRecord>(
            "dbo.usp_PurchasePrice_MasterHistoryPage",
            query,
            cancellationToken);

    public Task<PurchasePricePagedResult<ActualPoPriceHistoryRecord>> GetActualPoHistoryPageAsync(
        PurchasePriceHistoryQuery query,
        CancellationToken cancellationToken) =>
        QueryHistoryAsync<ActualPoPriceHistoryRecord>(
            "dbo.usp_PurchasePrice_ActualPoHistoryPage",
            query,
            cancellationToken);

    private async Task<PurchasePricePagedResult<T>> QueryHistoryAsync<T>(
        string procedure,
        PurchasePriceHistoryQuery query,
        CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(
            cancellationToken: cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("supplier_code", query.SupplierCode, DbType.AnsiString, size: 10);
        parameters.Add("item_code", query.ItemCode, DbType.AnsiString, size: 20);
        parameters.Add("po_um", query.PoUm, DbType.AnsiString, size: 3);
        parameters.Add("from_date", query.FromDate?.Date, DbType.Date);
        parameters.Add("to_date", query.ToDate?.Date, DbType.Date);
        parameters.Add("skip", query.Skip, DbType.Int32);
        parameters.Add("take", query.Take, DbType.Int32);

        using var results = await connection.QueryMultipleAsync(
            Command(procedure, parameters, cancellationToken));

        var data = (await results.ReadAsync<T>()).AsList();
        var total = await results.ReadSingleAsync<long>();

        return new PurchasePricePagedResult<T>
        {
            Data = data,
            Total = total
        };
    }

    private static DynamicParameters ResolutionParameters(
        string supplierCode,
        string itemCode,
        string poUm,
        DateTime? asOfDate)
    {
        var parameters = new DynamicParameters();
        parameters.Add("supplier_code", supplierCode, DbType.AnsiString, size: 10);
        parameters.Add("item_code", itemCode, DbType.AnsiString, size: 20);
        parameters.Add("po_um", poUm, DbType.AnsiString, size: 3);
        parameters.Add("as_of_date", asOfDate?.Date, DbType.Date);
        return parameters;
    }

    private static CommandDefinition Command(
        string procedure,
        object? parameters,
        CancellationToken cancellationToken) =>
        new(
            procedure,
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

    private static PurchasePriceException Translate(SqlException exception)
    {
        var message = exception.Message;
        var separator = message.IndexOf(':');
        var detail = separator >= 0 ? message[(separator + 1)..].Trim() : message;

        return message.StartsWith("PURCHASE_PRICE_CONFLICT:", StringComparison.OrdinalIgnoreCase)
            ? new PurchasePriceConflictException(detail, exception)
            : new PurchasePriceValidationException(
                message.StartsWith("PURCHASE_PRICE_VALIDATION:", StringComparison.OrdinalIgnoreCase)
                    ? detail
                    : "The purchase price operation was rejected by the database.",
                exception);
    }
}
