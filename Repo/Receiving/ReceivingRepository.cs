using System.Data;
using Dapper;
using HRIStwinbeeAPI.ConnectionData;
using HRIStwinbeeAPI.DTO.Receiving;
using HRIStwinbeeAPI.Repo.Interface.Receiving;

namespace HRIStwinbeeAPI.Repo.Receiving;

public sealed class ReceivingRepository(
    ISqlConnectionFactory connectionFactory) : IReceivingRepository
{
    /*
    |--------------------------------------------------------------------------
    | Receiving list
    |--------------------------------------------------------------------------
    */

    public async Task<PagedResult<ReceivingListItemDto>> QueryAsync(
        ReceivingQueryRequest request,
        CancellationToken cancellationToken)
    {
        await using var connection =
            await connectionFactory.CreateOpenConnectionAsync(
                cancellationToken: cancellationToken);

        var parameters = new DynamicParameters();

        parameters.Add(
            "date_from",
            request.DateFrom?.Date,
            DbType.Date);

        parameters.Add(
            "date_to",
            request.DateTo?.Date,
            DbType.Date);

        parameters.Add(
            "status",
            Normalize(request.Status),
            DbType.AnsiString,
            size: 10);

        //parameters.Add(
        //   "poNo",
        //   Normalize(request.Status),
        //   DbType.AnsiString,
        //   size: 10);

        parameters.Add(
            "trans_type",
            Normalize(request.TransType),
            DbType.AnsiString,
            size: 10);

        parameters.Add(
            "branchname",
            Normalize(request.Branchname),
            DbType.AnsiString,
            size: 50);

        parameters.Add(
            "whse_code",
            Normalize(request.WhseCode),
            DbType.AnsiString,
            size: 5);

        parameters.Add(
            "supp_code",
            Normalize(request.SuppCode),
            DbType.AnsiString,
            size: 8);

        parameters.Add(
            "search",
            Normalize(request.Search),
            DbType.AnsiString,
            size: 100);

        parameters.Add(
            "skip",
            Math.Max(0, request.Skip),
            DbType.Int32);

        parameters.Add(
            "take",
            Math.Clamp(request.Take, 1, 500),
            DbType.Int32);

        parameters.Add(
            "sort_field",
            Normalize(request.SortField) ?? "rrDate",
            DbType.AnsiString,
            size: 50);

        parameters.Add(
            "sort_dir",
            string.Equals(
                request.SortDir,
                "asc",
                StringComparison.OrdinalIgnoreCase)
                ? "ASC"
                : "DESC",
            DbType.AnsiString,
            size: 4);

        parameters.Add(
    "userId",
    request.UserId.Trim().ToUpperInvariant(),
    DbType.AnsiString,
    size: 10);

        var command = new CommandDefinition(
            "dbo.usp_Receiving_Query",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        using var results = await connection.QueryMultipleAsync(command);

        var data =
            (await results.ReadAsync<ReceivingListItemDto>())
            .AsList();

        var total = await results.ReadSingleAsync<long>();

        return new PagedResult<ReceivingListItemDto>
        {
            Data = data,
            Total = total
        };
    }

    /*
    |--------------------------------------------------------------------------
    | Get receiving document
    |--------------------------------------------------------------------------
    */

    public async Task<ReceivingDocumentDto?> GetByNoAsync(
        string rrNo,
        CancellationToken cancellationToken)
    {
        await using var connection =
            await connectionFactory.CreateOpenConnectionAsync(
                cancellationToken: cancellationToken);

        var parameters = new DynamicParameters();

        parameters.Add(
            "rr_no",
            rrNo.Trim(),
            DbType.AnsiStringFixedLength,
            size: 10);

        var command = new CommandDefinition(
            "dbo.usp_Receiving_GetByNo",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        using var results = await connection.QueryMultipleAsync(command);

        var header =
            await results.ReadSingleOrDefaultAsync<ReceivingDocumentDto>();

        if (header is null)
        {
            return null;
        }

        header.Details =
            (await results.ReadAsync<ReceivingDetailDto>())
            .AsList();

        return header;
    }

    /*
    |--------------------------------------------------------------------------
    | Supplier lookup
    |--------------------------------------------------------------------------
    */

    public Task<IReadOnlyList<SupplierLookupDto>> GetSuppliersAsync(
        CancellationToken cancellationToken) =>
        QueryLookupAsync<SupplierLookupDto>(
            "dbo.usp_Receiving_GetSuppliers",
            cancellationToken);

    /*
    |--------------------------------------------------------------------------
    | Branch lookup
    |--------------------------------------------------------------------------
    */

    public Task<IReadOnlyList<BranchLookupDto>> GetBranchesAsync(
        CancellationToken cancellationToken) =>
        QueryLookupAsync<BranchLookupDto>(
            "dbo.usp_Receiving_GetBranches",
            cancellationToken);

    /*
    |--------------------------------------------------------------------------
    | Warehouse lookup
    |--------------------------------------------------------------------------
    */

    public Task<IReadOnlyList<WarehouseLookupDto>> GetWarehousesAsync(
        CancellationToken cancellationToken) =>
        QueryLookupAsync<WarehouseLookupDto>(
            "dbo.usp_Receiving_GetWarehouses",
            cancellationToken);

    /*
    |--------------------------------------------------------------------------
    | Open PO lines
    |--------------------------------------------------------------------------
    */

    public async Task<PagedResult<OpenPoLineDto>> GetOpenPoLinesAsync(
        OpenPoQueryRequest request,
        CancellationToken cancellationToken)
    {
        await using var connection =
            await connectionFactory.CreateOpenConnectionAsync(
                cancellationToken: cancellationToken);

        var parameters = new DynamicParameters();

        parameters.Add(
            "supp_code",
            Normalize(request.SuppCode),
            DbType.AnsiString,
            size: 8);
        //  @whse_code = 'whse'
        parameters.Add(
          "whse_code",
          Normalize(request.WhseCode),
          DbType.AnsiString,
          size: 10);

        parameters.Add(
            "search",
            Normalize(request.Search),
            DbType.AnsiString,
            size: 100);

        parameters.Add(
            "skip",
            Math.Max(0, request.Skip),
            DbType.Int32);

        parameters.Add(
            "take",
            Math.Clamp(request.Take, 1, 200),
            DbType.Int32);

        var command = new CommandDefinition(
            "dbo.usp_Receiving_GetOpenPOLines",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        using var results = await connection.QueryMultipleAsync(command);

        var data =
            (await results.ReadAsync<OpenPoLineDto>())
            .AsList();

        var total = await results.ReadSingleAsync<long>();

        return new PagedResult<OpenPoLineDto>
        {
            Data = data,
            Total = total
        };
    }

    /*
    |--------------------------------------------------------------------------
    | Save / update / post receiving
    |--------------------------------------------------------------------------
    */

    public async Task<ReceivingSaveResultDto> UpsertBatchAsync(
        UpsertReceivingRequest request,
        CancellationToken cancellationToken)
    {
        await using var connection =
            await connectionFactory.CreateOpenConnectionAsync(
                cancellationToken: cancellationToken);

        var parameters = new DynamicParameters();

        parameters.Add(
            "rr_no",
            Normalize(request.RrNo),
            DbType.AnsiStringFixedLength,
            ParameterDirection.InputOutput,
            size: 10);

        parameters.Add(
            "rr_date",
            request.RrDate,
            DbType.DateTime2);

        parameters.Add(
            "branchname",
            request.Branchname.Trim(),
            DbType.AnsiString,
            size: 50);

        parameters.Add(
            "whse_code",
            request.WhseCode.Trim(),
            DbType.AnsiString,
            size: 5);

        /*
         * MODIFIED:
         * Reference is nullable, so do not call request.Reference.Trim().
         */

        parameters.Add(
            "reference",
            Normalize(request.Reference),
            DbType.AnsiString,
            size: 20);

        parameters.Add(
            "supp_code",
            Normalize(request.SuppCode),
            DbType.AnsiString,
            size: 8);

        //parameters.Add(
        //    "so_no",
        //    Normalize(request.SoNo),
        //    DbType.AnsiString,
        //    size: 10);

        //parameters.Add(
        //    "custid",
        //    request.CustId,
        //    DbType.Int32);

        parameters.Add(
            "target_status",
            request.TargetStatus.Trim().ToUpperInvariant(),
            DbType.AnsiStringFixedLength,
            size: 1);

        parameters.Add(
            "trans_type",
            request.TransType.Trim().ToUpperInvariant(),
            DbType.AnsiStringFixedLength,
            size: 3);

        parameters.Add(
            "user_id",
            request.UserId!.Trim(),
            DbType.AnsiString,
            size: 10);

        parameters.Add(
            "remarks",
            Normalize(request.Remarks),
            DbType.AnsiString,
            size: 100);

        /*
         * MODIFIED:
         * Convert hexadecimal rowVersion string into VARBINARY(8).
         */

        parameters.Add(
            "expected_row_version",
            ParseRowVersion(request.ExpectedRowVersion),
            DbType.Binary,
            size: 8);

        parameters.Add(
            "details",
            BuildDetailsTable(request.Details));

        var command = new CommandDefinition(
            "dbo.usp_Receiving_UpsertBatch",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        return await connection
            .QuerySingleAsync<ReceivingSaveResultDto>(command);
    }

    /*
    |--------------------------------------------------------------------------
    | Reverse / unpost receiving
    |--------------------------------------------------------------------------
    */

    public async Task<ReceivingSaveResultDto> ReverseAsync(
        string rrNo,
        ReverseReceivingRequest request,
        CancellationToken cancellationToken)
    {
        await using var connection =
            await connectionFactory.CreateOpenConnectionAsync(
                cancellationToken: cancellationToken);

        var parameters = new DynamicParameters();

        parameters.Add(
            "rr_no",
            rrNo.Trim(),
            DbType.AnsiStringFixedLength,
            size: 10);

        parameters.Add(
            "reversed_by",
            request.UserId!.Trim(),
            DbType.AnsiString,
            size: 10);

        parameters.Add(
            "reversal_reason",
            request.Reason.Trim(),
            DbType.String,
            size: 500);

        /*
         * MODIFIED:
         * Convert hexadecimal rowVersion string into VARBINARY(8).
         */

        parameters.Add(
            "expected_row_version",
            ParseRowVersion(request.ExpectedRowVersion),
            DbType.Binary,
            size: 8);

        var command = new CommandDefinition(
            "dbo.usp_Receiving_Reverse",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        return await connection
            .QuerySingleAsync<ReceivingSaveResultDto>(command);
    }

    /*
    |--------------------------------------------------------------------------
    | Generic lookup executor
    |--------------------------------------------------------------------------
    */

    private async Task<IReadOnlyList<T>> QueryLookupAsync<T>(
        string procedure,
        CancellationToken cancellationToken)
    {
        await using var connection =
            await connectionFactory.CreateOpenConnectionAsync(
                cancellationToken: cancellationToken);

        var command = new CommandDefinition(
            procedure,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        return (await connection.QueryAsync<T>(command)).AsList();
    }

    /*
    |--------------------------------------------------------------------------
    | Receiving detail TVP
    |--------------------------------------------------------------------------
    */

    private static SqlMapper.ICustomQueryParameter BuildDetailsTable(
        IEnumerable<ReceivingDetailUpsertDto> details)
    {
        var table = new DataTable();

        /*
         * Important:
         * The column names and order must match:
         * dbo.ReceivingDetailBatchType
         */

        table.Columns.Add("rr_seq", typeof(int));
        table.Columns.Add("po_line_no", typeof(int));
        table.Columns.Add("po_no", typeof(string));
        table.Columns.Add("item_code", typeof(string));
        table.Columns.Add("item_desc", typeof(string));
        table.Columns.Add("qty_rcv", typeof(decimal));
        table.Columns.Add("base_um", typeof(string));
        table.Columns.Add("po_um", typeof(string));
        table.Columns.Add("po_conv_factor", typeof(decimal));
        table.Columns.Add("unit_price", typeof(decimal));

        foreach (var detail in details.OrderBy(detail => detail.RrSeq))
        {
            table.Rows.Add(
                detail.RrSeq,

                detail.PoLineNo.HasValue
                    ? detail.PoLineNo.Value
                    : DBNull.Value,

                DbValue(detail.PoNo),

                DbValue(detail.ItemCode),

                DbValue(detail.ItemDesc),

                detail.QtyRcv,

                DbValue(detail.BaseUm),

                DbValue(detail.PoUm),

                detail.PoConvFactor,

                detail.UnitPrice);
        }

        return table.AsTableValuedParameter(
            "dbo.ReceivingDetailBatchType");
    }

    /*
    |--------------------------------------------------------------------------
    | Convert nullable string into database value
    |--------------------------------------------------------------------------
    */

    private static object DbValue(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? DBNull.Value
            : value.Trim();

    /*
    |--------------------------------------------------------------------------
    | Normalize nullable strings
    |--------------------------------------------------------------------------
    |
    | Important:
    | This method must appear only once in this class.
    |
    */

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();

    /*
    |--------------------------------------------------------------------------
    | Parse hexadecimal rowVersion
    |--------------------------------------------------------------------------
    |
    | Accepted examples:
    |
    | 0000000000001A2F
    | 0x0000000000001A2F
    |
    | Result:
    | byte[8] for SQL VARBINARY(8)
    |
    */

    private static byte[]? ParseRowVersion(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var rowVersion = value.Trim();

        /*
         * FORMAT 1:
         * SQL hexadecimal with 0x prefix
         *
         * Example:
         * 0x0000000000001A2F
         */
        if (rowVersion.StartsWith(
                "0x",
                StringComparison.OrdinalIgnoreCase))
        {
            rowVersion = rowVersion[2..];
        }

        /*
         * FORMAT 2:
         * Plain hexadecimal
         *
         * Example:
         * 0000000000001A2F
         */
        if (rowVersion.Length == 16 &&
            rowVersion.All(Uri.IsHexDigit))
        {
            return Convert.FromHexString(rowVersion);
        }

        /*
         * FORMAT 3:
         * Base64 generated by ASP.NET when byte[] is serialized to JSON
         *
         * Example:
         * AAAAAAAAGi8=
         */
        try
        {
            var bytes = Convert.FromBase64String(rowVersion);

            if (bytes.Length == 8)
            {
                return bytes;
            }
        }
        catch (FormatException)
        {
            // Continue to the clear validation error below.
        }

        throw new ArgumentException(
            "Invalid row version. Expected an 8-byte Base64 value or exactly 16 hexadecimal characters.",
            nameof(value));
    }

    /*
|--------------------------------------------------------------------------
| Item lookup for Non-PO receiving
|--------------------------------------------------------------------------
*/

    public async Task<IReadOnlyList<ReceivingItemLookupDto>> GetItemsAsync(
        string? search,
        string? whseCode,
        CancellationToken cancellationToken)
    {
        await using var connection =
            await connectionFactory.CreateOpenConnectionAsync(
                cancellationToken: cancellationToken);

        var parameters = new DynamicParameters();

        parameters.Add(
            "whse_code",
            Normalize(whseCode),
            DbType.AnsiString,
            size: 5);

        parameters.Add(
            "search",
            Normalize(search),
            DbType.AnsiString,
            size: 100);

        parameters.Add(
            "skip",
            0,
            DbType.Int32);

        parameters.Add(
            "take",
            200,
            DbType.Int32);

        var command = new CommandDefinition(
            "dbo.usp_Receiving_GetItems",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        return (await connection.QueryAsync<ReceivingItemLookupDto>(command))
            .AsList();
    }

    //public async Task<IReadOnlyList<SoVerificationDto>> VerifySoNoAsync(
    //string soNo,
    //CancellationToken cancellationToken)
    //{
    //    var cleanSoNo = Normalize(soNo);

    //    if (cleanSoNo is null)
    //    {
    //        return [];
    //    }

    //    await using var connection =
    //        await connectionFactory.CreateOpenConnectionAsync(
    //            cancellationToken: cancellationToken);

    //    var parameters = new DynamicParameters();

    //    parameters.Add(
    //        "so_no",
    //        cleanSoNo,
    //        DbType.AnsiString,
    //        size: 10);

    //    var command = new CommandDefinition(
    //        "dbo.usp_Receiving_VerifySONo",
    //        parameters,
    //        commandType: CommandType.StoredProcedure,
    //        cancellationToken: cancellationToken);

    //    return (
    //        await connection.QueryAsync<SoVerificationDto>(command)
    //    ).AsList();
    //}
}