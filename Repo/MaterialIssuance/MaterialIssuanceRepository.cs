using System.Data;
//using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using Dapper;
using HRIStwinbeeAPI.ConnectionData;
using HRIStwinbeeAPI.DTO.MaterialIssuance;
using HRIStwinbeeAPI.Repo.Interface.MaterialIssuance;

namespace HRIStwinbeeAPI.Repo.MaterialIssuance;

public sealed class MaterialIssuanceRepository(ISqlConnectionFactory factory) : IMaterialIssuanceRepository
{
    public async Task<MaterialIssuancePagedResult<MaterialIssuanceListItemDto>> QueryAsync(MaterialIssuanceQueryRequest r, CancellationToken token)
    {
        await using var cn = await factory.CreateOpenConnectionAsync(cancellationToken: token);
        var rows = (await cn.QueryAsync<MaterialIssuanceListItemDto>(new CommandDefinition(
            "dbo.usp_MaterialIssuance_Query", new
            {
                date_from = r.DateFrom?.Date,
                date_to = r.DateTo?.Date,
                status = Clean(r.Status) ?? "ALL",
                source_type = Clean(r.SourceType) ?? "ALL",
                whse_code = Clean(r.WhseCode),
                search = Clean(r.Search),
                skip = r.Skip,
                take = r.Take,
                userId = Clean(r.UserId)
            },
            commandType: CommandType.StoredProcedure, cancellationToken: token))).AsList();
        return new() { Data = rows, Total = rows.Count == 0 ? 0 : rows[0].TotalCount };
    }

    public async Task<MaterialIssuanceDocumentDto?> GetByNoAsync(string issueNo, CancellationToken token)
    {
        await using var cn = await factory.CreateOpenConnectionAsync(cancellationToken: token);
        using var grid = await cn.QueryMultipleAsync(new CommandDefinition("dbo.usp_MaterialIssuance_GetByNo",
            new { issue_no = issueNo.Trim() }, commandType: CommandType.StoredProcedure, cancellationToken: token));
        var doc = await grid.ReadSingleOrDefaultAsync<MaterialIssuanceDocumentDto>();
        if (doc is null) return null;
        doc.Details = (await grid.ReadAsync<MaterialIssuanceDetailDto>()).AsList();
        doc.Lots = (await grid.ReadAsync<MaterialIssuanceLotDto>()).AsList();
        doc.GlLines = (await grid.ReadAsync<MaterialIssuanceGlLineDto>()).AsList();
        return doc;
    }

    public Task<IReadOnlyList<MaterialIssuanceWarehouseDto>> GetWarehousesAsync(string? search, CancellationToken token) =>
        QueryList<MaterialIssuanceWarehouseDto>("dbo.usp_MaterialIssuance_GetWarehouses", new { search = Clean(search) }, token);


    public Task<IReadOnlyList<MaterialIssuanceSupplierDto>> GetSuppliersAsync(string? search, CancellationToken token) =>
        QueryList<MaterialIssuanceSupplierDto>("dbo.usp_MaterialIssuance_GetSuppliers", new { search = Clean(search) }, token);


    public async Task<IReadOnlyList<MaterialIssuanceSoVerificationDto>>
    VerifySoNoAsync(
        string soNo,
        CancellationToken token)
    {
        var cleanSoNo = Clean(soNo);

        if (string.IsNullOrWhiteSpace(cleanSoNo))
        {
            return Array.Empty<MaterialIssuanceSoVerificationDto>();
        }

        await using var connection =
            await factory.CreateOpenConnectionAsync(
                cancellationToken: token);

        var parameters = new DynamicParameters();

        parameters.Add(
            "so_no",
            cleanSoNo,
            DbType.AnsiString,
            size: 10);

        var command = new CommandDefinition(
            commandText: "dbo.usp_MaterialIssuance_VerifySONo",
            parameters: parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: token);

        var rows =
            await connection.QueryAsync<MaterialIssuanceSoVerificationDto>(
                command);

        return rows.AsList();
    }

    public Task<IReadOnlyList<MaterialIssuanceWorkCenterDto>> GetWorkCentersAsync(
    string? search,
    CancellationToken token) =>
    QueryList<MaterialIssuanceWorkCenterDto>(
        "dbo.usp_WorkCenter_Lookup",
        new
        {
            search = Clean(search)
        },
        token);


    public async Task<IReadOnlyList<MaterialLookupDto>>
       GetAvailableMaterialsAsync(
           string whseCode,
           string? search,
           CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(whseCode))
        {
            throw new ArgumentException(
                "Warehouse code is required.",
                nameof(whseCode));
        }

        await using var connection =
            await factory.CreateOpenConnectionAsync(
                cancellationToken: token);

        var command = new CommandDefinition(
            commandText:
                "dbo.usp_MaterialIssuance_GetAvailableMaterials",
            parameters: new
            {
                whse_code = whseCode.Trim(),
                search = Clean(search),
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: token);

        var rows =
            await connection.QueryAsync<MaterialLookupDto>(
                command);

        return rows.AsList();
    }
    public Task<IReadOnlyList<FifoPreviewDto>> GetFifoPreviewAsync(string whseCode, string itemCode, decimal? baseQty, CancellationToken token) =>
        QueryList<FifoPreviewDto>("dbo.usp_MaterialIssuance_GetFifoPreview", new { whse_code = whseCode.Trim(), item_code = itemCode.Trim(), base_qty = baseQty }, token);

    public async Task<MaterialIssuanceSaveResultDto> UpsertAsync(
    UpsertMaterialIssuanceRequest r,
    CancellationToken token)
    {
        try
        {
            if (r is null)
            {
                throw new ArgumentNullException(
                    nameof(r),
                    "Material issuance request is required.");
            }

            if (r.Details is null || r.Details.Count == 0)
            {
                throw new ArgumentException(
                    "At least one material issuance detail is required.",
                    nameof(r));
            }

            var table = new DataTable();

            table.Columns.Add("issue_seq", typeof(int));
            table.Columns.Add("item_code", typeof(string));
            table.Columns.Add("item_desc", typeof(string));
            table.Columns.Add("qty_iss", typeof(decimal));
            table.Columns.Add("issue_um", typeof(string));
            table.Columns.Add("base_um", typeof(string));
            table.Columns.Add("conv_factor", typeof(decimal));
            table.Columns.Add("cc_code", typeof(string));
            table.Columns.Add("remarks", typeof(string));

            foreach (var detail in r.Details)
            {
                if (detail is null)
                {
                    throw new ArgumentException(
                        "Material issuance contains an invalid detail row.",
                        nameof(r));
                }

                table.Rows.Add(
                    detail.IssueSeq,
                    detail.ItemCode.Trim(),
                    detail.ItemDesc.Trim(),
                    detail.QtyIss,
                    detail.IssueUm.Trim(),
                    detail.BaseUm.Trim(),
                    detail.ConvFactor,
                    (object?)Clean(detail.CcCode) ?? DBNull.Value,
                    (object?)Clean(detail.Remarks) ?? DBNull.Value);
            }

            var parameters = new DynamicParameters();

            parameters.Add(
                "issue_no",
                Clean(r.IssueNo),
                DbType.AnsiString,
                ParameterDirection.InputOutput,
                size: 10);

            parameters.Add(
                "expected_row_version",
                ParseRowVersion(r.ExpectedRowVersion),
                DbType.Binary,
                size: 8);

            parameters.Add(
                "issue_date",
                r.IssueDate.Date,
                DbType.Date);

            parameters.Add(
                "whse_code",
                r.WhseCode.Trim(),
                DbType.AnsiString,
                size: 5);

            parameters.Add(
                "source_type",
                r.SourceType.Trim(),
                DbType.AnsiStringFixedLength,
                size: 3);

            parameters.Add(
                "reference",
                r.Reference.Trim(),
                DbType.AnsiString,
                size: 20);

            parameters.Add(
                "source_no",
                Clean(r.SourceNo),
                DbType.AnsiString,
                size: 20);

            parameters.Add(
                "so_no",
                Clean(r.SoNo),
                DbType.AnsiString,
                size: 10);

            parameters.Add(
                "custid",
                r.CustId,
                DbType.Int32);

            parameters.Add(
                "cc_code",
                Clean(r.CcCode),
                DbType.AnsiString,
                size: 5);

            parameters.Add(
                "wc_code",
                Clean(r.WcCode),
                DbType.AnsiString,
                size: 5);

            parameters.Add(
                "supp_code",
                Clean(r.SuppCode),
                DbType.AnsiString,
                size: 6);

            parameters.Add(
                "remarks",
                Clean(r.Remarks),
                DbType.AnsiString,
                size: 500);

            parameters.Add(
                "target_status",
                r.TargetStatus.Trim(),
                DbType.AnsiStringFixedLength,
                size: 1);

            parameters.Add(
                "user_id",
                Clean(r.UserId),
                DbType.AnsiString,
                size: 10);

            parameters.Add(
                "details",
                table.AsTableValuedParameter(
                    "dbo.MaterialIssuanceDetailBatchType"));

            await using var connection =
                await factory.CreateOpenConnectionAsync(
                    cancellationToken: token);

            var command = new CommandDefinition(
                commandText: "dbo.usp_MaterialIssuance_UpsertBatch",
                parameters: parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: token);

            return await connection
                .QuerySingleAsync<MaterialIssuanceSaveResultDto>(
                    command);
        }
        catch (OperationCanceledException)
        {
            // Preserve request cancellation behavior.
            throw;
        }
        catch (ArgumentNullException)
        {
            // Preserve validation details for the controller.
            throw;
        }
        catch (ArgumentException)
        {
            // Includes invalid row version and invalid request values.
            throw;
        }
        catch (SqlException)
        {
            /*
             * Important:
             * Do not wrap SqlException because the controller checks
             * SQL error numbers such as 51100, 51110, and others.
             */
            throw;
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                "An unexpected error occurred while saving the material issuance.",
                exception);
        }
    }

    public async Task<MaterialIssuanceSaveResultDto> ReverseAsync(string issueNo, ReverseMaterialIssuanceRequest r, CancellationToken token)
    {
        await using var cn = await factory.CreateOpenConnectionAsync(cancellationToken: token);
        return await cn.QuerySingleAsync<MaterialIssuanceSaveResultDto>(new CommandDefinition("dbo.usp_MaterialIssuance_Reverse",
            new
            {
                issue_no = issueNo.Trim(),
                expected_row_version = ParseRowVersion(r.ExpectedRowVersion),
                reason = r.Reason.Trim(),
                user_id = r.UserId
            },
            commandType: CommandType.StoredProcedure, cancellationToken: token));
    }

    private async Task<IReadOnlyList<T>> QueryList<T>(string proc, object args, CancellationToken token)
    {
        await using var cn = await factory.CreateOpenConnectionAsync(cancellationToken: token);
        return (await cn.QueryAsync<T>(new CommandDefinition(proc, args, commandType: CommandType.StoredProcedure, cancellationToken: token))).AsList();
    }
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static byte[]? ParseRowVersion(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var v = value.Trim(); if (v.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) v = v[2..];
        if (v.Length == 16 && v.All(Uri.IsHexDigit)) return Convert.FromHexString(v);
        try { var bytes = Convert.FromBase64String(v); if (bytes.Length == 8) return bytes; } catch (FormatException) { }
        throw new ArgumentException("Expected an 8-byte Base64 or 16-character hexadecimal row version.", nameof(value));
    }
}
