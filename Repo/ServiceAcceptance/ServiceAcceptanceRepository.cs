using System.Data;
using Dapper;
using HRIStwinbeeAPI.ConnectionData;
using HRIStwinbeeAPI.DTO.ServiceAcceptance;
using HRIStwinbeeAPI.Repo.Interface.ServiceAcceptance;
using Microsoft.Data.SqlClient;

namespace HRIStwinbeeAPI.Repo.ServiceAcceptance;

public sealed class ServiceAcceptanceRepository(
    ISqlConnectionFactory connectionFactory)
    : IServiceAcceptanceRepository
{
    /* ============================================================
       LIST
       ============================================================ */

    public async Task<
        ServiceAcceptancePagedResultDto<
            ServiceAcceptanceListItemDto>>
        QueryAsync(
            ServiceAcceptanceQueryDto query,
            CancellationToken token)
    {
        await using var connection =
            await connectionFactory
                .CreateOpenConnectionAsync(
                    cancellationToken:
                        token);

        var parameters =
            new DynamicParameters();

        parameters.Add(
            "@from_date",
            query.FromDate?.Date,
            DbType.Date);

        parameters.Add(
            "@to_date",
            query.ToDate?.Date,
            DbType.Date);

        parameters.Add(
            "@status",
            Clean(query.Status) ??
            "ALL",
            DbType.String,
            size: 20);

        parameters.Add(
            "@branchname",
            Clean(query.BranchName),
            DbType.String,
            size: 50);

        parameters.Add(
            "@search",
            Clean(query.Search),
            DbType.String,
            size: 150);

        parameters.Add(
            "@skip",
            Math.Max(
                0,
                query.Skip));

        parameters.Add(
            "@take",
            Math.Clamp(
                query.Take,
                1,
                200));

        parameters.Add(
            "@sort_field",
            Clean(query.SortField) ??
            "acceptanceDate",
            DbType.String,
            size: 40);

        parameters.Add(
            "@sort_dir",
            string.Equals(
                query.SortDir,
                "asc",
                StringComparison.OrdinalIgnoreCase)
                ? "asc"
                : "desc");

        var rows =
            (
                await connection
                    .QueryAsync<
                        ServiceAcceptanceListItemDto>(
                        new CommandDefinition(
                            "dbo.usp_ServiceAcceptance_List",
                            parameters,
                            commandType:
                                CommandType.StoredProcedure,
                            cancellationToken:
                                token))
            )
            .AsList();

        return new()
        {
            Data =
                rows,

            Total =
                rows
                    .FirstOrDefault()
                    ?.TotalCount
                ??
                0
        };
    }


    /* ============================================================
       GET
       ============================================================ */

    public async Task<ServiceAcceptanceDocumentDto?>
        GetAsync(
            string documentNo,
            CancellationToken token)
    {
        await using var connection =
            await connectionFactory
                .CreateOpenConnectionAsync(
                    cancellationToken:
                        token);

        using var grid =
            await connection
                .QueryMultipleAsync(
                    new CommandDefinition(
                        "dbo.usp_ServiceAcceptance_GetByNo",
                        new
                        {
                            service_acceptance_no =
                                documentNo
                        },
                        commandType:
                            CommandType.StoredProcedure,
                        cancellationToken:
                            token));

        var header =
            await grid
                .ReadFirstOrDefaultAsync<
                    ServiceAcceptanceHeaderDto>();

        if (header is null)
        {
            return null;
        }

        return new()
        {
            Header =
                header,

            Details =
                (
                    await grid
                        .ReadAsync<
                            ServiceAcceptanceDetailDto>()
                )
                .AsList(),

            Checklist =
                (
                    await grid
                        .ReadAsync<
                            ServiceAcceptanceChecklistDto>()
                )
                .AsList()
        };
    }


    /* ============================================================
       SAVE
       ============================================================ */

    public async Task<ServiceAcceptanceCommandResultDto>
        SaveAsync(
            ServiceAcceptanceSaveDto request,
            CancellationToken token)
    {
        try
        {
            Validate(
                request);

            await using var connection =
                await connectionFactory
                    .CreateOpenConnectionAsync(
                        cancellationToken:
                            token);

            var h =
                request.Header;

            var p =
                new DynamicParameters();

            p.Add(
                "@service_acceptance_no",
                Clean(
                    h.ServiceAcceptanceNo),
                DbType.String,
                size: 30);

            p.Add(
                "@acceptance_date",
                h.AcceptanceDate.Date,
                DbType.Date);

            /*
             * NO @po_no HERE ANYMORE.
             */

            p.Add(
                "@supp_code",
                h.SuppCode.Trim(),
                DbType.String,
                size: 10);

            p.Add(
                "@service_type",
                h.ServiceType
                    .Trim()
                    .ToUpperInvariant(),
                DbType.String,
                size: 30);

            p.Add(
                "@branchname",
                h.BranchName.Trim(),
                DbType.String,
                size: 50);

            p.Add(
                "@department_code",
                Clean(
                    h.DepartmentCode),
                DbType.String,
                size: 30);

            p.Add(
                "@project_no",
                Clean(
                    h.ProjectNo),
                DbType.String,
                size: 30);

            p.Add(
                "@jo_no",
                Clean(
                    h.JoNo),
                DbType.String,
                size: 30);

            p.Add(
                "@service_location",
                Clean(
                    h.ServiceLocation),
                DbType.String,
                size: 150);

            p.Add(
                "@service_period_from",
                h.ServicePeriodFrom?.Date,
                DbType.Date);

            p.Add(
                "@service_period_to",
                h.ServicePeriodTo?.Date,
                DbType.Date);

            p.Add(
                "@requested_by",
                Clean(
                    h.RequestedBy),
                DbType.String,
                size: 100);

            p.Add(
                "@remarks",
                Clean(
                    h.Remarks),
                DbType.String,
                size: 500);

            p.Add(
                "@user_id",
                h.UserId.Trim(),
                DbType.String,
                size: 50);

            p.Add(
                "@expected_row_version",
                Clean(
                    h.RowVersion),
                DbType.String,
                size: 18);

            p.Add(
                "@details",
                BuildDetails(
                        request.Details)
                    .AsTableValuedParameter(
                        "dbo.ServiceAcceptanceDetailType"));

            p.Add(
                "@checklist",
                BuildChecklist(
                        request.Checklist)
                    .AsTableValuedParameter(
                        "dbo.ServiceAcceptanceChecklistType"));

            return await connection
                .QuerySingleAsync<
                    ServiceAcceptanceCommandResultDto>(
                    new CommandDefinition(
                        "dbo.usp_ServiceAcceptance_Save",
                        p,
                        commandType:
                            CommandType.StoredProcedure,
                        cancellationToken:
                            token));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (SqlException ex)
        {
            throw new InvalidOperationException(
                $"Service Acceptance save failed. SQL Server error: {ex.Message}",
                ex);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Service Acceptance save failed: {ex.Message}",
                ex);
        }
    }


    /* ============================================================
       WORKFLOW
       ============================================================ */

    public async Task<ServiceAcceptanceCommandResultDto>
        TransitionAsync(
            string documentNo,
            string action,
            ServiceAcceptanceWorkflowDto request,
            CancellationToken token)
    {
        /*
         * Submit remains here only for backward compatibility.
         *
         * New Angular UI does not call submit.
         */
        var procedures =
            new Dictionary<
                string,
                string>(
                StringComparer.OrdinalIgnoreCase)
            {
                ["submit"] =
                    "dbo.usp_ServiceAcceptance_Submit",

                ["accept"] =
                    "dbo.usp_ServiceAcceptance_Accept",

                ["post"] =
                    "dbo.usp_ServiceAcceptance_Post",

                ["reverse"] =
                    "dbo.usp_ServiceAcceptance_Reverse"
            };

        if (
            !procedures.TryGetValue(
                action,
                out var procedure)
        )
        {
            throw new ArgumentException(
                "Unsupported Service Acceptance action.",
                nameof(action));
        }

        await using var connection =
            await connectionFactory
                .CreateOpenConnectionAsync(
                    cancellationToken:
                        token);

        var p =
            new DynamicParameters();

        p.Add(
            "@service_acceptance_no",
            documentNo.Trim(),
            DbType.String,
            size: 30);

        p.Add(
            "@user_id",
            request.UserId.Trim(),
            DbType.String,
            size: 50);

        p.Add(
            "@expected_row_version",
            Clean(
                request.RowVersion),
            DbType.String,
            size: 18);

        if (
            action.Equals(
                "reverse",
                StringComparison.OrdinalIgnoreCase)
        )
        {
            p.Add(
                "@reverse_reason",
                Clean(
                    request.Reason),
                DbType.String,
                size: 500);
        }

        return await connection
            .QuerySingleAsync<
                ServiceAcceptanceCommandResultDto>(
                new CommandDefinition(
                    procedure,
                    p,
                    commandType:
                        CommandType.StoredProcedure,
                    cancellationToken:
                        token));
    }


    /* ============================================================
       OPEN SERVICE PO LINES
       ============================================================ */

    public Task<
        IReadOnlyList<
            ServiceOpenPoLineLookupDto>>
        GetOpenPoLinesAsync(
            string supplierCode,
            string? branchName,
            string? excludeAcceptanceNo,
            string? search,
            CancellationToken token)
    {
        if (
            string.IsNullOrWhiteSpace(
                supplierCode)
        )
        {
            throw new ArgumentException(
                "Supplier code is required.",
                nameof(supplierCode));
        }

        return LookupAsync<
            ServiceOpenPoLineLookupDto>(
                "dbo.usp_ServiceAcceptance_OpenPoLineLookup",
                new
                {
                    supp_code =
                        supplierCode.Trim(),

                    branchname =
                        Clean(
                            branchName),

                    exclude_acceptance_no =
                        Clean(
                            excludeAcceptanceNo),

                    search =
                        Clean(
                            search)
                },
                token);
    }


    /* ============================================================
       SUPPLIERS
       ============================================================ */

    public Task<
        IReadOnlyList<
            ServiceSupplierLookupDto>>
        GetSuppliersAsync(
            string? search,
            CancellationToken token)
        =>
        LookupAsync<
            ServiceSupplierLookupDto>(
                "dbo.usp_ServiceAcceptance_SupplierLookup",
                new
                {
                    search =
                        Clean(
                            search)
                },
                token);


    /* ============================================================
       BRANCHES
       ============================================================ */

    public Task<
        IReadOnlyList<
            ServiceBranchLookupDto>>
        GetBranchesAsync(
            CancellationToken token)
        =>
        LookupAsync<
            ServiceBranchLookupDto>(
                "dbo.usp_ServiceAcceptance_BranchLookup",
                null,
                token);


    /* ============================================================
       TAX
       ============================================================ */

    public Task<
        IReadOnlyList<
            ServiceTaxSchemeLookupDto>>
        GetTaxSchemesAsync(
            CancellationToken token)
        =>
        LookupAsync<
            ServiceTaxSchemeLookupDto>(
                "dbo.usp_ServiceAcceptance_TaxSchemeLookup",
                null,
                token);


    /* ============================================================
       VP
       ============================================================ */

    public Task<
        IReadOnlyList<
            ServiceVpSourceDto>>
        GetVpSourcesAsync(
            string? supplierCode,
            string? branchName,
            string? search,
            CancellationToken token)
        =>
        LookupAsync<
            ServiceVpSourceDto>(
                "dbo.usp_ServiceAcceptance_VpSourceLookup",
                new
                {
                    supp_code =
                        Clean(
                            supplierCode),

                    branchname =
                        Clean(
                            branchName),

                    search =
                        Clean(
                            search)
                },
                token);


    /* ============================================================
       GENERIC LOOKUP
       ============================================================ */

    private async Task<IReadOnlyList<T>>
        LookupAsync<T>(
            string procedure,
            object? args,
            CancellationToken token)
    {
        await using var connection =
            await connectionFactory
                .CreateOpenConnectionAsync(
                    cancellationToken:
                        token);

        return
            (
                await connection
                    .QueryAsync<T>(
                        new CommandDefinition(
                            procedure,
                            args,
                            commandType:
                                CommandType.StoredProcedure,
                            cancellationToken:
                                token))
            )
            .AsList();
    }


    /* ============================================================
       DETAIL TVP
       ============================================================ */

    private static DataTable
        BuildDetails(
            IEnumerable<
                ServiceAcceptanceDetailSaveDto> details)
    {
        var table =
            new DataTable();

        /*
         * IMPORTANT:
         * Column order must exactly match the SQL TVP.
         */
        table.Columns.Add(
            "line_seq",
            typeof(int));

        table.Columns.Add(
            "po_no",
            typeof(string));

        table.Columns.Add(
            "po_seq",
            typeof(int));

        table.Columns.Add(
            "item_code",
            typeof(string));

        table.Columns.Add(
            "service_description",
            typeof(string));

        table.Columns.Add(
            "unit_of_measure",
            typeof(string));

        table.Columns.Add(
            "po_qty",
            typeof(decimal));

        table.Columns.Add(
            "remaining_qty",
            typeof(decimal));

        table.Columns.Add(
            "accepted_qty",
            typeof(decimal));

        table.Columns.Add(
            "rate",
            typeof(decimal));

        table.Columns.Add(
            "tax_scheme_code",
            typeof(string));

        table.Columns.Add(
            "remarks",
            typeof(string));


        foreach (
            var d
            in details)
        {
            table.Rows.Add(
                d.LineSeq,

                d.PoNo.Trim(),

                d.PoSeq,

                Db(
                    d.ItemCode),

                d.ServiceDescription
                    .Trim(),

                Db(
                    d.UnitOfMeasure),

                d.PoQty,

                d.RemainingQty,

                d.AcceptedQty,

                d.Rate,

                d.TaxSchemeCode
                    .Trim()
                    .ToUpperInvariant(),

                Db(
                    d.Remarks));
        }

        return table;
    }


    /* ============================================================
       CHECKLIST TVP
       ============================================================ */

    private static DataTable
        BuildChecklist(
            IEnumerable<
                ServiceAcceptanceChecklistSaveDto> rows)
    {
        var table =
            new DataTable();

        table.Columns.Add(
            "line_seq",
            typeof(int));

        table.Columns.Add(
            "checklist_code",
            typeof(string));

        table.Columns.Add(
            "checklist_description",
            typeof(string));

        table.Columns.Add(
            "is_required",
            typeof(bool));

        table.Columns.Add(
            "is_completed",
            typeof(bool));

        table.Columns.Add(
            "remarks",
            typeof(string));


        foreach (
            var x
            in rows)
        {
            table.Rows.Add(
                x.LineSeq,

                x.ChecklistCode
                    .Trim(),

                x.ChecklistDescription
                    .Trim(),

                x.IsRequired,

                x.IsCompleted,

                Db(
                    x.Remarks));
        }

        return table;
    }


    /* ============================================================
       VALIDATION
       ============================================================ */

    private static void Validate(
        ServiceAcceptanceSaveDto request)
    {
        if (
            request.Details.Count ==
            0
        )
        {
            throw new ArgumentException(
                "At least one Service PO line is required.");
        }


        if (
            request.Details
                .GroupBy(
                    x =>
                        x.LineSeq)
                .Any(
                    x =>
                        x.Count() >
                        1)
        )
        {
            throw new ArgumentException(
                "Service line sequence values must be unique.");
        }


        /*
         * IMPORTANT:
         * Duplicate is PO + PO line.
         */
        if (
            request.Details
                .GroupBy(
                    x => new
                    {
                        PoNo =
                            x.PoNo
                                .Trim()
                                .ToUpperInvariant(),

                        x.PoSeq
                    })
                .Any(
                    x =>
                        x.Count() >
                        1)
        )
        {
            throw new ArgumentException(
                "A Service PO line can appear only once.");
        }


        if (
            request.Details.Any(
                x =>
                    string.IsNullOrWhiteSpace(
                        x.PoNo)

                    ||

                    x.PoSeq <=
                    0)
        )
        {
            throw new ArgumentException(
                "Every Service line requires a valid PO number and PO line.");
        }


        if (
            request.Header.ServicePeriodFrom >
            request.Header.ServicePeriodTo
        )
        {
            throw new ArgumentException(
                "Service Period To cannot be earlier than Service Period From.");
        }


        if (
            request.Details.Any(
                x =>
                    x.AcceptedQty <=
                    0

                    ||

                    x.AcceptedQty >
                    x.RemainingQty

                    ||

                    x.Rate <
                    0)
        )
        {
            throw new ArgumentException(
                "Accepted quantity and rate are outside the allowed PO balance.");
        }
    }


    private static object Db(
        string? value)
        =>
        Clean(value) is { } clean
            ? clean
            : DBNull.Value;


    private static string? Clean(
        string? value)
        =>
        string.IsNullOrWhiteSpace(
            value)
            ? null
            : value.Trim();
}