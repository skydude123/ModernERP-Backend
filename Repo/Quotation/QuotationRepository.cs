
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Dapper;
using HRIStwinbeeAPI.ConnectionData;
using HRIStwinbeeAPI.DTO.Quotation;
using HRIStwinbeeAPI.Repo.Interface.Quotation;
using Microsoft.Extensions.Logging;

namespace HRIStwinbeeAPI.Repo.Quotation;

public sealed class QuotationRepository : IQuotationRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<QuotationRepository> _logger;

    public QuotationRepository(
        ISqlConnectionFactory connectionFactory,
        ILogger<QuotationRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    // ==========================================================
    // GET QUOTATION HEADER
    // ==========================================================

    public async Task<QuotationHeaderDto?> GetHeaderAsync(
        string sqNo,
        CancellationToken cancellationToken = default)
    {
        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync(
                "TWINBEE",
                cancellationToken);

        var parameters = new DynamicParameters();

        parameters.Add(
            "@sqno",
            sqNo.Trim(),
            DbType.StringFixedLength,
            size: 10);

        var command = new CommandDefinition(
            "dbo.usp_Quotation_GetHeader",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        return await connection
            .QueryFirstOrDefaultAsync<QuotationHeaderDto>(command);
    }

    // ==========================================================
    // GET QUOTATION DETAILS
    // ==========================================================

    public async Task<IReadOnlyList<QuotationDetailDto>>
        GetDetailsAsync(
            string sqNo,
            string vatType,
            CancellationToken cancellationToken = default)
    {
        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync(
                "TWINBEE",
                cancellationToken);

        var parameters = new DynamicParameters();

        parameters.Add(
            "@sqno",
            sqNo.Trim(),
            DbType.AnsiStringFixedLength,
            size: 10);

        parameters.Add(
            "@vat_type",
            vatType,
            DbType.AnsiString,
            size: 10);

        var command = new CommandDefinition(
            "dbo.getNewQuotationDtltoFillBySqNoVer2",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var rows = await connection
            .QueryAsync<QuotationDetailDto>(command);

        return rows
            .OrderBy(x => x.SeqLineNo)
            .ToList();
    }

    // ==========================================================
    // GET ADDITIONAL CHARGES
    // ==========================================================

    public async Task<QuotationAdditionalChargeDto?>
        GetAdditionalChargesAsync(
            string sqNo,
            CancellationToken cancellationToken = default)
    {
        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync(
                "TWINBEE",
                cancellationToken);

        var parameters = new DynamicParameters();

        parameters.Add(
            "@sqno",
            sqNo.Trim(),
            DbType.AnsiString,
            size: 10);

        var command = new CommandDefinition(
            "dbo.GetAdditionalChargeQuotation",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        return await connection
            .QueryFirstOrDefaultAsync<QuotationAdditionalChargeDto>(
                command);
    }

    // ==========================================================
    // TERMS LOOKUP
    // ==========================================================

    public async Task<IReadOnlyList<QuotationTermDto>>
        GetTermsAsync(
            CancellationToken cancellationToken = default)
    {
        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync(
                "TWINBEE",
                cancellationToken);

        var command = new CommandDefinition(
            "dbo.getTermsList",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var rows = await connection
            .QueryAsync<QuotationTermDto>(command);

        return rows.ToList();
    }

    // ==========================================================
    // ARTISTS LOOKUP
    // ==========================================================

    public async Task<IReadOnlyList<QuotationArtistDto>>
        GetArtistsAsync(
            string branchName,
            CancellationToken cancellationToken = default)
    {
        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync(
                "TWINBEE",
                cancellationToken);

        var parameters = new DynamicParameters();

        parameters.Add(
            "@branchname",
            branchName,
            DbType.AnsiString,
            size: 10);

        var command = new CommandDefinition(
            "dbo.getArtistName",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var rows = await connection
            .QueryAsync<QuotationArtistDto>(command);

        return rows.ToList();
    }

    // ==========================================================
    // CUSTOMERS LOOKUP
    // ==========================================================

    public async Task<IReadOnlyList<QuotationCustomerDto>>
        GetCustomersAsync(
            CancellationToken cancellationToken = default)
    {
        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync(
                "TWINBEE",
                cancellationToken);

        var command = new CommandDefinition(
            "Customer.getCustomerList",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var rows = await connection
            .QueryAsync<QuotationCustomerDto>(command);

        return rows.ToList();
    }

    // ==========================================================
    // QUOTATION LIST
    // ==========================================================

    public async Task<IReadOnlyList<QuotationListDto>>
        GetListAsync(
            string userId,
            string branchName,
            CancellationToken cancellationToken = default)
    {
        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync(
                "TWINBEE",
                cancellationToken);

        var parameters = new DynamicParameters();

        parameters.Add(
            "@userid",
            userId,
            DbType.AnsiString,
            size: 10);

        parameters.Add(
            "@branchname",
            branchName,
            DbType.AnsiString,
            size: 10);

        var command = new CommandDefinition(
            "dbo.getNewQuotationBySalesBranchNO",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var rows = await connection
            .QueryAsync<QuotationListDto>(command);

        return rows.ToList();
    }

    // ==========================================================
    // UNIT OF MEASURE
    // ==========================================================

    public async Task<IReadOnlyList<string>>
        GetUnitsOfMeasureAsync(
            CancellationToken cancellationToken = default)
    {
        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync(
                "TWINBEE",
                cancellationToken);

        var command = new CommandDefinition(
            "dbo.UnitOfMeasureLIST",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var rows = await connection
            .QueryAsync<string>(command);

        return rows
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();
    }

    // ==========================================================
    // WARRANTY LOOKUP
    // ==========================================================

    public async Task<IReadOnlyList<QuotationLookupOptionDto>>
        GetWarrantyAsync(
            CancellationToken cancellationToken = default)
    {
        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync(
                "TWINBEE",
                cancellationToken);

        var command = new CommandDefinition(
            "dbo.usp_QuotationWarrantyLookup_GetAll",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var rows = await connection
            .QueryAsync<QuotationLookupOptionDto>(command);

        return rows.ToList();
    }

    // ==========================================================
    // DELIVERY LOOKUP
    // ==========================================================

    public async Task<IReadOnlyList<QuotationLookupOptionDto>>
        GetDeliveryAsync(
            CancellationToken cancellationToken = default)
    {
        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync(
                "TWINBEE",
                cancellationToken);

        var command = new CommandDefinition(
            "dbo.usp_QuotationDeliveryLookup_GetAll",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var rows = await connection
            .QueryAsync<QuotationLookupOptionDto>(command);

        return rows.ToList();
    }


    // ==========================================================
    // SERVICE LOOKUP
    // GET /api/quotation/lookups/services
    // ==========================================================

    public async Task<IReadOnlyList<QuotationServiceLookupDto>>
        GetServicesAsync(
            CancellationToken cancellationToken = default)
    {
        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync(
                "TWINBEE",
                cancellationToken);

        var parameters = new DynamicParameters();

        parameters.Add(
            "@IncludeInactive",
            false,
            DbType.Boolean);

        var command = new CommandDefinition(
            "dbo.usp_QuotationServiceLookup_GetAll",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var rows = await connection
            .QueryAsync<QuotationServiceLookupDto>(command);

        return rows
            .Where(x => x.IsActive)
            .OrderBy(x =>
                string.Equals(
                    x.ServiceName?.Trim(),
                    "OTHERS",
                    StringComparison.OrdinalIgnoreCase)
                    ? 1
                    : 0)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.ServiceName)
            .ToList();
    }


    // ==========================================================
    // CONFIRM QUOTATION
    // dbo.usp_Quotation_Confirm
    // ==========================================================

    public async Task<QuotationConfirmResultDto> ConfirmAsync(
        string sqNo,
        string confirmRef,
        DateTime confirmDate,
        string authenticatedUserId,
        string authenticatedBranch,
        bool isAdministrator,
        CancellationToken cancellationToken = default)
    {
        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync(
                "TWINBEE",
                cancellationToken);

        var parameters = new DynamicParameters();

        parameters.Add(
            "@SqNo",
            sqNo.Trim(),
            DbType.StringFixedLength,
            size: 10);

        parameters.Add(
            "@ConfirmRef",
            confirmRef.Trim(),
            DbType.AnsiString,
            size: 50);

        parameters.Add(
            "@ConfirmDate",
            confirmDate.Date,
            DbType.Date);

        parameters.Add(
            "@ActorUserId",
            authenticatedUserId.Trim(),
            DbType.AnsiString,
            size: 10);

        parameters.Add(
            "@ActorBranch",
            authenticatedBranch.Trim(),
            DbType.AnsiString,
            size: 15);

        parameters.Add(
            "@IsAdministrator",
            isAdministrator,
            DbType.Boolean);

        var command = new CommandDefinition(
            "dbo.usp_Quotation_Confirm",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var result = await connection
            .QuerySingleAsync<QuotationConfirmResultDto>(
                command);

        return result;
    }



    // ==========================================================
    // SINGLE QUOTATION UPSERT
    // INSERT + UPDATE
    // ==========================================================

    public async Task<QuotationUpsertResultDto> UpsertAsync(
        QuotationUpsertRequest request,
        string authenticatedUserId,
        string authenticatedBranch,
        bool isAdministrator,
        CancellationToken cancellationToken = default)
    {
        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync(
                "TWINBEE",
                cancellationToken);

        // ======================================================
        // AUTHORIZE UPDATE
        // ======================================================

        if (request.Action == "UPDATE")
        {
            const string authorizationSql = @"
SELECT
    RTRIM(userid) AS UserId,
    RTRIM(branchname) AS BranchName,
    RTRIM(status) AS Status
FROM dbo.QUOTATIONHDR
WHERE sqno = @SqNo;";

            var existing = await connection
                .QuerySingleOrDefaultAsync<QuotationAuthorizationRow>(
                    new CommandDefinition(
                        authorizationSql,
                        new
                        {
                            request.SqNo
                        },
                        cancellationToken: cancellationToken));

            if (existing is null)
            {
                throw new KeyNotFoundException(
                    "Quotation was not found.");
            }

            // Admin can edit quotations in other branches.
            // Non-admin must own quotation and branch.
            if (!isAdministrator)
            {
                var sameUser = string.Equals(
                    existing.UserId?.Trim(),
                    authenticatedUserId,
                    StringComparison.OrdinalIgnoreCase);

                var sameBranch = string.Equals(
                    existing.BranchName?.Trim(),
                    authenticatedBranch,
                    StringComparison.OrdinalIgnoreCase);

                if (!sameUser || !sameBranch)
                {
                    throw new UnauthorizedAccessException(
                        "You are not authorized to edit this quotation.");
                }
            }

            if (!string.Equals(
                    existing.Status?.Trim(),
                    "NEW",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Only NEW quotations may be updated.");
            }
        }

        // ======================================================
        // BUILD DETAIL TABLE-VALUED PARAMETER
        // ======================================================

        var detailTable = CreateQuotationDetailTable(
            request.Details ??
            new List<QuotationUpsertDetailDto>());

        // ======================================================
        // BUILD STORED PROCEDURE PARAMETERS
        // ======================================================

        var parameters = new DynamicParameters();

        parameters.Add(
            "@Action",
            request.Action,
            DbType.AnsiString,
            size: 10);

        parameters.Add(
            "@SaveScope",
            request.SaveScope,
            DbType.AnsiString,
            size: 10);

        parameters.Add(
            "@SqNo",
            request.Action == "INSERT"
                ? null
                : request.SqNo,
            DbType.StringFixedLength,
            size: 10);

        parameters.Add(
            "@CustomerId",
            request.CustomerId,
            DbType.Int32);

        parameters.Add(
            "@ProcessId",
            request.ProcessId,
            DbType.AnsiStringFixedLength,
            size: 3);

        parameters.Add(
            "@VatType",
            request.VatType,
            DbType.StringFixedLength,
            size: 20);

        parameters.Add(
            "@TermId",
            request.TermId,
            DbType.AnsiStringFixedLength,
            size: 3);

        parameters.Add(
            "@FinId",
            request.FinId,
            DbType.Int32);

        parameters.Add(
            "@Warranty",
            request.Warranty,
            DbType.AnsiString,
            size: 20);

        parameters.Add(
            "@Delivery",
            request.Delivery,
            DbType.AnsiString,
            size: 20);

        parameters.Add(
            "@SqDate",
            request.SqDate,
            DbType.DateTime);

        parameters.Add(
            "@AcPerson",
            request.AcPerson,
            DbType.AnsiString,
            size: 30);

        parameters.Add(
            "@ContactP",
            request.ContactP,
            DbType.AnsiString,
            size: 60);

        parameters.Add(
            "@Remarks",
            request.Remarks,
            DbType.AnsiString);

        // Security:
        // Always use authenticated values,
        // never client-supplied creator or branch.
        parameters.Add(
            "@BranchName",
            authenticatedBranch,
            DbType.AnsiString,
            size: 15);

        parameters.Add(
            "@UserId",
            authenticatedUserId,
            DbType.StringFixedLength,
            size: 10);

        parameters.Add(
            "@ArtCode",
            request.ArtCode,
            DbType.Int32);

        // Header percentage stored as fraction.
        parameters.Add(
            "@Discnt",
            request.Discnt,
            DbType.Decimal,
            precision: 10,
            scale: 6);

        parameters.Add(
            "@Details",
            detailTable.AsTableValuedParameter(
                "dbo.QuotationDetailUpsertTypeV2"));

        // ======================================================
        // EXECUTE ONE SQL UPSERT
        // ======================================================

        var command = new CommandDefinition(
            "dbo.usp_Quotation_Upsert",
            parameters,
            commandType: CommandType.StoredProcedure,
            commandTimeout: 60,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Quotation UPSERT requested: Action={Action}, " +
            "SqNo={SqNo}, Scope={Scope}, User={User}",
            request.Action,
            request.SqNo,
            request.SaveScope,
            authenticatedUserId);

        var result = await connection
            .QuerySingleAsync<QuotationUpsertResultDto>(
                command);

        if (!result.Success ||
            string.IsNullOrWhiteSpace(result.SqNo))
        {
            throw new InvalidOperationException(
                "Quotation UPSERT was not confirmed.");
        }

        _logger.LogInformation(
            "Quotation UPSERT completed: " +
            "Operation={Operation}, SqNo={SqNo}",
            result.Operation,
            result.SqNo);

        return result;
    }

    // ==========================================================
    // CREATE DETAILS TABLE-VALUED PARAMETER
    //
    // IMPORTANT:
    // Order must match dbo.QuotationDetailUpsertType
    // ==========================================================

    private static DataTable CreateQuotationDetailTable(
        IEnumerable<QuotationUpsertDetailDto> details)
    {
        var table = new DataTable();

        table.Columns.Add(
            "SeqLineNo",
            typeof(int));

        table.Columns.Add(
            "Description",
            typeof(string));

        table.Columns.Add(
            "Size",
            typeof(string));

        table.Columns.Add(
            "UM",
            typeof(string));

        table.Columns.Add(
            "Qty",
            typeof(decimal));

        table.Columns.Add(
            "UnitCost",
            typeof(decimal));

        table.Columns.Add(
            "TotAmt",
            typeof(decimal));

        table.Columns.Add(
            "Discnt",
            typeof(decimal));

        table.Columns.Add(
            "Materials",
            typeof(string));

        table.Columns.Add(
            "VAT",
            typeof(decimal));

        table.Columns.Add(
            "ServiceId",
            typeof(string));

        table.Columns.Add(
            "FinalFin",
            typeof(string));

        foreach (var item in details)
        {
            table.Rows.Add(
                item.SeqLineNo,
                item.Description,
                (object?)item.Size ?? DBNull.Value,
                (object?)item.UM ?? DBNull.Value,
                item.Qty,
                item.UnitCost,
                item.TotAmt,
                item.Discnt,
                (object?)item.Materials ?? DBNull.Value,
                item.VAT,
                (object?)item.ServiceId ?? DBNull.Value,
                (object?)item.FinalFin ?? DBNull.Value
            );
        }

        return table;
    }

    // ==========================================================
    // INTERNAL AUTHORIZATION MODEL
    // ==========================================================

    private sealed class QuotationAuthorizationRow
    {
        public string? UserId { get; set; }

        public string? BranchName { get; set; }

        public string? Status { get; set; }
    }
}
