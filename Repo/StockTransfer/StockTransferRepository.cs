using System.Data;
using Dapper;
using HRIStwinbeeAPI.ConnectionData;
using HRIStwinbeeAPI.DTO.StockTransfer;
using HRIStwinbeeAPI.Repo.Interface.StockTransfer;

namespace HRIStwinbeeAPI.Repo.StockTransfer;

public sealed class StockTransferRepository(ISqlConnectionFactory factory) : IStockTransferRepository
{
    public async Task<StockTransferPagedResult<StockTransferListItemDto>> QueryAsync(
        StockTransferQueryRequest request,
        CancellationToken token)
    {
        await using var connection = await factory.CreateOpenConnectionAsync(cancellationToken: token);
        var rows = (await connection.QueryAsync<StockTransferListItemDto>(new CommandDefinition(
            "dbo.usp_StockTransfer_Query",
            new
            {
                date_from = request.DateFrom?.Date,
                date_to = request.DateTo?.Date,
                status = Clean(request.Status) ?? "ALL",
                source_whse_code = Clean(request.SourceWhseCode),
                destination_whse_code = Clean(request.DestinationWhseCode),
                search = Clean(request.Search),
                skip = request.Skip,
                take = request.Take,
                userId = Clean(request.UserId)
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: token))).AsList();

        return new StockTransferPagedResult<StockTransferListItemDto>
        {
            Data = rows,
            Total = rows.Count == 0 ? 0 : rows[0].TotalCount
        };
    }

    public async Task<StockTransferDocumentDto?> GetByNoAsync(string stNo, CancellationToken token)
    {
        await using var connection = await factory.CreateOpenConnectionAsync(cancellationToken: token);
        using var grid = await connection.QueryMultipleAsync(new CommandDefinition(
            "dbo.usp_StockTransfer_GetByNo",
            new { st_no = stNo.Trim() },
            commandType: CommandType.StoredProcedure,
            cancellationToken: token));

        var document = await grid.ReadSingleOrDefaultAsync<StockTransferDocumentDto>();
        if (document is null)
        {
            return null;
        }

        document.Details = (await grid.ReadAsync<StockTransferDetailDto>()).AsList();
        return document;
    }

    public Task<IReadOnlyList<StockTransferWarehouseDto>> GetWarehousesAsync(
        string? search,
        CancellationToken token) =>
        QueryListAsync<StockTransferWarehouseDto>(
            "dbo.usp_StockTransfer_GetWarehouses",
            new { search = Clean(search) },
            token);

    public Task<IReadOnlyList<StockTransferSourceItemDto>> GetSourceItemsAsync(
        string sourceWhseCode,
        string? search,
        CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(sourceWhseCode))
        {
            throw new ArgumentException("Source warehouse is required.", nameof(sourceWhseCode));
        }

        return QueryListAsync<StockTransferSourceItemDto>(
            "dbo.usp_StockTransfer_GetSourceItems",
            new
            {
                whse_code = sourceWhseCode.Trim(),
                search = Clean(search)
            },
            token);
    }

    public async Task<StockTransferSaveResultDto> UpsertAsync(
        UpsertStockTransferRequest request,
        CancellationToken token)
    {
        byte[]? expectedRowVersion = null;
        if (!string.IsNullOrWhiteSpace(request.ExpectedRowVersion))
        {
            try
            {
                expectedRowVersion = Convert.FromBase64String(request.ExpectedRowVersion);
            }
            catch (FormatException exception)
            {
                throw new ArgumentException("Expected rowversion is not valid Base64.", nameof(request), exception);
            }

            if (expectedRowVersion.Length != 8)
            {
                throw new ArgumentException("Expected rowversion must contain 8 bytes.", nameof(request));
            }
        }

        var details = new DataTable();
        details.Columns.Add("st_seq", typeof(int));
        details.Columns.Add("item_code", typeof(string));
        details.Columns.Add("qty_iss", typeof(decimal));
        details.Columns.Add("xfer_um", typeof(string));
        details.Columns.Add("conv_factor", typeof(decimal));
        details.Columns.Add("base_um", typeof(string));

        foreach (var detail in request.Details)
        {
            details.Rows.Add(
                detail.StSeq,
                detail.ItemCode.Trim(),
                detail.QtyIss,
                detail.XferUm.Trim(),
                detail.ConvFactor,
                detail.BaseUm.Trim());
        }

        var parameters = new DynamicParameters();
        parameters.Add("st_no", Clean(request.StNo), DbType.String, ParameterDirection.InputOutput, 10);
        parameters.Add("expected_row_version", expectedRowVersion, DbType.Binary, size: 8);
        parameters.Add("st_date", request.StDate.Date, DbType.Date);
        parameters.Add("source_whse_code", request.SourceWhseCode.Trim(), DbType.String, size: 5);
        parameters.Add("destination_whse_code", request.DestinationWhseCode.Trim(), DbType.String, size: 5);
        parameters.Add("reference", request.Reference.Trim(), DbType.String, size: 20);
        parameters.Add("received_by", Clean(request.ReceivedBy), DbType.String, size: 10);
        parameters.Add("received_date", request.ReceivedDate, DbType.DateTime);
        parameters.Add("remarks", Clean(request.Remarks), DbType.String, size: 500);
        parameters.Add("target_status", request.TargetStatus.Trim().ToUpperInvariant(), DbType.AnsiStringFixedLength, size: 1);
        parameters.Add("user_id", request.UserId!.Trim(), DbType.String, size: 10);
        parameters.Add("details", details.AsTableValuedParameter("dbo.StockTransferDetailBatchType"));

        await using var connection = await factory.CreateOpenConnectionAsync(cancellationToken: token);
        return await connection.QuerySingleAsync<StockTransferSaveResultDto>(new CommandDefinition(
            "dbo.usp_StockTransfer_UpsertBatch",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: token));
    }

    public async Task<StockTransferReverseResultDto> ReverseAsync(
    ReverseStockTransferRequest request,
    CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(request.StNo))
        {
            throw new ArgumentException(
                "Stock Transfer number is required.",
                nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            throw new ArgumentException(
                "User ID is required.",
                nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Reason) ||
            request.Reason.Trim().Length < 5)
        {
            throw new ArgumentException(
                "Reverse reason must contain at least 5 characters.",
                nameof(request));
        }

        byte[]? expectedRowVersion = null;

        if (!string.IsNullOrWhiteSpace(request.ExpectedRowVersion))
        {
            try
            {
                expectedRowVersion =
                    Convert.FromBase64String(
                        request.ExpectedRowVersion);
            }
            catch (FormatException exception)
            {
                throw new ArgumentException(
                    "Expected rowversion is not valid Base64.",
                    nameof(request),
                    exception);
            }

            if (expectedRowVersion.Length != 8)
            {
                throw new ArgumentException(
                    "Expected rowversion must contain 8 bytes.",
                    nameof(request));
            }
        }

        var parameters =
            new DynamicParameters();

        parameters.Add(
            "st_no",
            request.StNo.Trim(),
            DbType.String,
            size: 10);

        parameters.Add(
            "expected_row_version",
            expectedRowVersion,
            DbType.Binary,
            size: 8);

        parameters.Add(
            "reason",
            request.Reason.Trim(),
            DbType.String,
            size: 500);

        parameters.Add(
            "user_id",
            request.UserId.Trim(),
            DbType.String,
            size: 10);

        await using var connection =
            await factory.CreateOpenConnectionAsync(
                cancellationToken: token);

        return await connection
            .QuerySingleAsync<StockTransferReverseResultDto>(
                new CommandDefinition(
                    "dbo.usp_StockTransfer_Reverse",
                    parameters,
                    commandType:
                        CommandType.StoredProcedure,
                    cancellationToken:
                        token));
    }

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
