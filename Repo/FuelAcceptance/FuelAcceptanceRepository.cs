using System.Data;

using Dapper;

using HRIStwinbeeAPI.ConnectionData;
using HRIStwinbeeAPI.DTO.FuelAcceptance;
using HRIStwinbeeAPI.Repo.Interface.FuelAcceptance;

namespace HRIStwinbeeAPI.Repo.FuelAcceptance;


public sealed class FuelAcceptanceRepository(
    ISqlConnectionFactory connectionFactory)
    : IFuelAcceptanceRepository
{
    /* ============================================================
       LIST
       ============================================================ */

    public async Task<
        FuelPagedResultDto<FuelAcceptanceListItemDto>>
        QueryAsync(
            FuelAcceptanceQueryDto query,
            CancellationToken cancellationToken)
    {
        await using var connection =
            await connectionFactory
                .CreateOpenConnectionAsync(
                    cancellationToken:
                        cancellationToken);


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
            Clean(query.Status) ?? "ALL",
            DbType.String,
            size: 3);


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
                query.Skip,
                0),
            DbType.Int32);


        parameters.Add(
            "@take",
            Math.Clamp(
                query.Take,
                1,
                200),
            DbType.Int32);


        parameters.Add(
            "@sort_field",
            Clean(query.SortField)
            ?? "acceptanceDate",
            DbType.String,
            size: 40);


        parameters.Add(
            "@sort_dir",
            string.Equals(
                query.SortDir,
                "asc",
                StringComparison.OrdinalIgnoreCase)
                ? "asc"
                : "desc",
            DbType.String,
            size: 4);


        var rows =
            (
                await connection.QueryAsync<
                    FuelAcceptanceListItemDto>(
                    new CommandDefinition(
                        "dbo.usp_FuelAcceptance_List",
                        parameters,
                        commandType:
                            CommandType.StoredProcedure,
                        cancellationToken:
                            cancellationToken))
            )
            .AsList();


        return new FuelPagedResultDto<
            FuelAcceptanceListItemDto>
        {
            Data = rows,

            Total =
                rows.FirstOrDefault()
                    ?.TotalCount
                ?? 0
        };
    }


    /* ============================================================
       GET DOCUMENT
       ============================================================ */

    public async Task<FuelAcceptanceDocumentDto?>
        GetAsync(
            string fuelAcceptanceNo,
            CancellationToken cancellationToken)
    {
        await using var connection =
            await connectionFactory
                .CreateOpenConnectionAsync(
                    cancellationToken:
                        cancellationToken);


        using var result =
            await connection.QueryMultipleAsync(
                new CommandDefinition(
                    "dbo.usp_FuelAcceptance_GetByNo",

                    new
                    {
                        fuel_acceptance_no =
                            fuelAcceptanceNo
                    },

                    commandType:
                        CommandType.StoredProcedure,

                    cancellationToken:
                        cancellationToken));


        var header =
            await result
                .ReadFirstOrDefaultAsync<
                    FuelAcceptanceHeaderDto>();


        if (header is null)
        {
            return null;
        }


        var details =
            (
                await result
                    .ReadAsync<
                        FuelAcceptanceDetailDto>()
            )
            .AsList();


        return new FuelAcceptanceDocumentDto
        {
            Header = header,

            Details = details
        };
    }


    /* ============================================================
       SAVE / UPDATE
       ============================================================ */

    public async Task<FuelAcceptanceCommandResultDto>
        SaveAsync(
            FuelAcceptanceSaveDto request,
            string userId,
            CancellationToken cancellationToken)
    {
        ValidateSave(request);


        await using var connection =
            await connectionFactory
                .CreateOpenConnectionAsync(
                    cancellationToken:
                        cancellationToken);


        var header =
            request.Header;


        var parameters =
            new DynamicParameters();


        parameters.Add(
            "@fuel_acceptance_no",
            Clean(
                header.FuelAcceptanceNo),
            DbType.String,
            size: 30);


        parameters.Add(
            "@acceptance_date",
            header.AcceptanceDate.Date,
            DbType.Date);


        /* ========================================================
           LEGACY HEADER PO

           SQL still accepts @po_no temporarily.

           It is no longer authoritative.

           SQL derives the legacy header PO from @details:
               one distinct PO -> that PO
               multiple PO     -> NULL
           ======================================================== */

        parameters.Add(
            "@po_no",
            null,
            DbType.String,
            size: 15);


        /* ========================================================
           SUPPLIER IS NOW AUTHORITATIVE AT HEADER
           ======================================================== */

        parameters.Add(
            "@supp_code",
            header.SuppCode.Trim(),
            DbType.String,
            size: 10);


        parameters.Add(
            "@branchname",
            header.BranchName.Trim(),
            DbType.String,
            size: 50);


        parameters.Add(
            "@reference",
            header.Reference.Trim(),
            DbType.String,
            size: 20);


        /* ========================================================
           OPTIONAL HEADER PARAMETERS
           ======================================================== */

        parameters.Add(
            "@department_code",
            null,
            DbType.String,
            size: 30);


        parameters.Add(
            "@project_no",
            null,
            DbType.String,
            size: 30);


        parameters.Add(
            "@jo_no",
            null,
            DbType.String,
            size: 30);


        parameters.Add(
            "@vehicle_code",
            Clean(
                header.VehicleNo),
            DbType.String,
            size: 30);


        parameters.Add(
            "@plate_no",
            Clean(
                header.PlateNo),
            DbType.String,
            size: 20);


        parameters.Add(
            "@driver_employee_id",
            null,
            DbType.String,
            size: 30);


        parameters.Add(
            "@driver_name",
            Clean(
                header.DriverName),
            DbType.String,
            size: 100);


        parameters.Add(
            "@purpose",
            Clean(
                header.Purpose),
            DbType.String,
            size: 250);


        parameters.Add(
            "@remarks",
            Clean(
                header.Remarks),
            DbType.String,
            size: 500);


        parameters.Add(
            "@user_id",
            userId.Trim(),
            DbType.String,
            size: 50);


        parameters.Add(
            "@expected_row_version",
            Clean(
                header.RowVersion),
            DbType.String,
            size: 18);


        /* ========================================================
           MULTI-PO TVP
           ======================================================== */

        parameters.Add(
            "@details",

            BuildDetails(
                request.Details)

            .AsTableValuedParameter(
                "dbo.FuelAcceptanceDetailUpsertType"));


        return await connection
            .QuerySingleAsync<
                FuelAcceptanceCommandResultDto>(
                new CommandDefinition(
                    "dbo.usp_FuelAcceptance_SaveUpsert",

                    parameters,

                    commandType:
                        CommandType.StoredProcedure,

                    cancellationToken:
                        cancellationToken));
    }


    /* ============================================================
       POST
       ============================================================ */

    public Task<FuelAcceptanceCommandResultDto>
        PostAsync(
            string fuelAcceptanceNo,
            string rowVersion,
            string userId,
            CancellationToken cancellationToken) =>
        ExecuteWorkflowAsync(
            "dbo.usp_FuelAcceptance_Post",
            fuelAcceptanceNo,
            rowVersion,
            userId,
            null,
            cancellationToken);


    /* ============================================================
       REVERSE
       ============================================================ */

    public Task<FuelAcceptanceCommandResultDto>
        ReverseAsync(
            string fuelAcceptanceNo,
            string rowVersion,
            string reason,
            string userId,
            CancellationToken cancellationToken) =>
        ExecuteWorkflowAsync(
            "dbo.usp_FuelAcceptance_Reverse",
            fuelAcceptanceNo,
            rowVersion,
            userId,
            reason,
            cancellationToken);


    /* ============================================================
       NEW:
       OPEN PO LINES BY SUPPLIER
       ============================================================ */

    public Task<
        IReadOnlyList<FuelPoLineLookupDto>>
        GetOpenPoLinesAsync(
            string supplierCode,
            string? excludeAcceptanceNo,
            string? search,
            CancellationToken cancellationToken) =>
        LookupAsync<FuelPoLineLookupDto>(
            "dbo.usp_FuelAcceptance_GetOpenPoLines",

            new
            {
                supp_code =
                    Clean(
                        supplierCode),

                exclude_acceptance_no =
                    Clean(
                        excludeAcceptanceNo),

                search =
                    Clean(
                        search)
            },

            cancellationToken);


    /* ============================================================
       SUPPLIERS
       ============================================================ */

    public Task<
        IReadOnlyList<FuelSupplierLookupDto>>
        GetSuppliersAsync(
            string? search,
            CancellationToken cancellationToken) =>
        LookupAsync<FuelSupplierLookupDto>(
            "dbo.usp_FuelAcceptance_SupplierLookup",

            new
            {
                search =
                    Clean(
                        search)
            },

            cancellationToken);


    /* ============================================================
       BRANCHES
       ============================================================ */

    public Task<
        IReadOnlyList<FuelBranchLookupDto>>
        GetBranchesAsync(
            string? search,
            CancellationToken cancellationToken) =>
        LookupAsync<FuelBranchLookupDto>(
            "dbo.usp_FuelAcceptance_BranchLookup",

            new
            {
                search =
                    Clean(
                        search)
            },

            cancellationToken);


    /* ============================================================
       TAX SCHEMES
       ============================================================ */

    public Task<
        IReadOnlyList<FuelTaxSchemeLookupDto>>
        GetTaxSchemesAsync(
            CancellationToken cancellationToken) =>
        LookupAsync<FuelTaxSchemeLookupDto>(
            "dbo.usp_FuelAcceptance_TaxSchemeLookup",
            null,
            cancellationToken);


    /* ============================================================
       OPEN FOR VP

       Uses the new DB procedure finalized earlier.
       ============================================================ */

    public Task<
        IReadOnlyList<FuelVpSourceDto>>
        GetOpenForVpAsync(
            string? supplierCode,
            string? branchName,
            string? search,
            CancellationToken cancellationToken) =>
        LookupAsync<FuelVpSourceDto>(
            "dbo.usp_FuelAcceptance_OpenForVP",

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

            cancellationToken);


    /* ============================================================
       WORKFLOW EXECUTOR
       ============================================================ */

    private async Task<
        FuelAcceptanceCommandResultDto>
        ExecuteWorkflowAsync(
            string procedure,
            string fuelAcceptanceNo,
            string rowVersion,
            string userId,
            string? reverseReason,
            CancellationToken cancellationToken)
    {
        await using var connection =
            await connectionFactory
                .CreateOpenConnectionAsync(
                    cancellationToken:
                        cancellationToken);


        var parameters =
            new DynamicParameters();


        parameters.Add(
            "@fuel_acceptance_no",
            fuelAcceptanceNo,
            DbType.String,
            size: 30);


        parameters.Add(
            "@user_id",
            userId,
            DbType.String,
            size: 50);


        parameters.Add(
            "@expected_row_version",
            rowVersion,
            DbType.String,
            size: 18);


        if (reverseReason is not null)
        {
            parameters.Add(
                "@reverse_reason",
                reverseReason,
                DbType.String,
                size: 500);
        }


        return await connection
            .QuerySingleAsync<
                FuelAcceptanceCommandResultDto>(
                new CommandDefinition(
                    procedure,

                    parameters,

                    commandType:
                        CommandType.StoredProcedure,

                    cancellationToken:
                        cancellationToken));
    }


    /* ============================================================
       GENERIC LOOKUP
       ============================================================ */

    private async Task<IReadOnlyList<T>>
        LookupAsync<T>(
            string procedure,
            object? parameters,
            CancellationToken cancellationToken)
    {
        await using var connection =
            await connectionFactory
                .CreateOpenConnectionAsync(
                    cancellationToken:
                        cancellationToken);


        return
            (
                await connection.QueryAsync<T>(
                    new CommandDefinition(
                        procedure,

                        parameters,

                        commandType:
                            CommandType.StoredProcedure,

                        cancellationToken:
                            cancellationToken))
            )
            .AsList();
    }


    /* ============================================================
       BUILD DETAILS TVP

       CRITICAL:

       Column order must EXACTLY match:

       dbo.FuelAcceptanceDetailUpsertType

       New beginning is:

           line_seq
           po_no
           po_line_no
           item_code
           item_desc
           qty_po
           po_um
           base_um
           po_conv_factor
           ...
       ============================================================ */

    private static DataTable BuildDetails(
        IEnumerable<FuelAcceptanceDetailSaveDto> details)
    {
        var table =
            new DataTable();


        /* ========================================================
           MULTI-PO SOURCE
           ======================================================== */

        table.Columns.Add(
            "line_seq",
            typeof(int));


        /*
         * NEW:
         * Must be immediately after line_seq.
         */
        table.Columns.Add(
            "po_no",
            typeof(string));


        table.Columns.Add(
            "po_line_no",
            typeof(int));


        table.Columns.Add(
            "item_code",
            typeof(string));


        table.Columns.Add(
            "item_desc",
            typeof(string));


        table.Columns.Add(
            "qty_po",
            typeof(decimal));


        table.Columns.Add(
            "po_um",
            typeof(string));


        table.Columns.Add(
            "base_um",
            typeof(string));


        table.Columns.Add(
            "po_conv_factor",
            typeof(decimal));


        /* ========================================================
           INTERNAL FUEL COLUMNS
           ======================================================== */

        table.Columns.Add(
            "fuel_date",
            typeof(DateTime));


        table.Columns.Add(
            "vehicle_code",
            typeof(string));


        table.Columns.Add(
            "plate_no",
            typeof(string));


        table.Columns.Add(
            "driver_employee_id",
            typeof(string));


        table.Columns.Add(
            "driver_name",
            typeof(string));


        table.Columns.Add(
            "fuel_station",
            typeof(string));


        table.Columns.Add(
            "invoice_no",
            typeof(string));


        table.Columns.Add(
            "receipt_no",
            typeof(string));


        table.Columns.Add(
            "has_receipt",
            typeof(bool));


        table.Columns.Add(
            "no_receipt_reason",
            typeof(string));


        table.Columns.Add(
            "fuel_type",
            typeof(string));


        table.Columns.Add(
            "liters",
            typeof(decimal));


        table.Columns.Add(
            "unit_price",
            typeof(decimal));


        table.Columns.Add(
            "tax_scheme_code",
            typeof(string));


        table.Columns.Add(
            "account_id",
            typeof(string));


        table.Columns.Add(
            "project_no",
            typeof(string));


        table.Columns.Add(
            "jo_no",
            typeof(string));


        table.Columns.Add(
            "route_destination",
            typeof(string));


        table.Columns.Add(
            "odometer_before",
            typeof(decimal));


        table.Columns.Add(
            "odometer_after",
            typeof(decimal));


        table.Columns.Add(
            "attachment_reference",
            typeof(string));


        table.Columns.Add(
            "remarks",
            typeof(string));


        /* ========================================================
           ROWS
           ======================================================== */

        foreach (var detail in details)
        {
            table.Rows.Add(

                /* line_seq */
                detail.LineSeq,


                /* =================================================
                   NEW: po_no
                   ================================================= */

                detail.PoNo.Trim(),


                /* po_line_no */
                detail.PoLineNo.HasValue
                    ? detail.PoLineNo.Value
                    : DBNull.Value,


                /* item_code */
                Db(
                    detail.ItemCode),


                /* item_desc */
                Db(
                    detail.ItemDesc),


                /* qty_po */
                detail.QtyPo.HasValue
                    ? detail.QtyPo.Value
                    : DBNull.Value,


                /* po_um */
                Db(
                    detail.PoUm),


                /* base_um */
                Db(
                    detail.BaseUm),


                /* po_conv_factor */
                detail.PoConvFactor.HasValue
                    ? detail.PoConvFactor.Value
                    : DBNull.Value,


                /* fuel_date */
                detail.FuelDate.Date,


                /*
                 * Internal line-level vehicle values
                 * currently not supplied by Angular.
                 */
                DBNull.Value,

                DBNull.Value,

                DBNull.Value,

                DBNull.Value,


                /*
                 * Fuel Station hidden.
                 *
                 * SQL TVP currently defines NOT NULL,
                 * therefore blank string.
                 */
                string.IsNullOrWhiteSpace(
                    detail.FuelStation)

                    ? string.Empty

                    : detail.FuelStation.Trim(),


                /* invoice_no */
                DBNull.Value,


                /* receipt_no */
                Db(
                    detail.ReceiptNo),


                /* has_receipt */
                detail.HasReceipt,


                /* no_receipt_reason */
                Db(
                    detail.NoReceiptReason),


                /* fuel_type */
                detail.FuelType.Trim(),


                /* liters */
                detail.Liters,


                /* unit_price */
                detail.UnitPrice,


                /*
                 * Tax scheme:
                 * hidden current workflow = NV.
                 */
                detail.HasReceipt

                    ? detail.TaxSchemeCode
                        .Trim()
                        .ToUpperInvariant()

                    : "NV",


                /* account_id */
                DBNull.Value,


                /* project_no */
                DBNull.Value,


                /* jo_no */
                DBNull.Value,


                /* route_destination */
                DBNull.Value,


                /* odometer_before */
                DBNull.Value,


                /* odometer_after */
                DBNull.Value,


                /* attachment_reference */
                DBNull.Value,


                /* remarks */
                Db(
                    detail.Remarks)
            );
        }


        return table;
    }


    /* ============================================================
       SAVE VALIDATION
       ============================================================ */

    private static void ValidateSave(
        FuelAcceptanceSaveDto request)
    {
        ArgumentNullException
            .ThrowIfNull(request);


        ArgumentNullException
            .ThrowIfNull(
                request.Header);


        /* ========================================================
           DOCUMENT-LEVEL VALIDATION
           ======================================================== */

        if (request.Details.Count == 0)
        {
            throw new ArgumentException(
                "At least one Fuel PO line is required.");
        }


        if (
            request.Header.AcceptanceDate ==
            default)
        {
            throw new ArgumentException(
                "Acceptance Date is required.");
        }


        /*
         * IMPORTANT:
         *
         * Header.PoNo is NO LONGER required.
         *
         * Supplier is now the primary header source.
         */
        if (
            string.IsNullOrWhiteSpace(
                request.Header.SuppCode))
        {
            throw new ArgumentException(
                "Supplier is required.");
        }


        if (
            string.IsNullOrWhiteSpace(
                request.Header.BranchName))
        {
            throw new ArgumentException(
                "Branch is required.");
        }


        if (
              string.IsNullOrWhiteSpace(
                request.Header.Reference))
        {
            throw new ArgumentException(
                "Reference is required.");
        }


        if (
            string.IsNullOrWhiteSpace(
                request.Header.PlateNo))
        {
            throw new ArgumentException(
                "Plate number is required.");
        }


        if (
            string.IsNullOrWhiteSpace(
                request.Header.DriverName))
        {
            throw new ArgumentException(
                "Driver name is required.");
        }


        /* ========================================================
           LINE SEQUENCE MUST BE UNIQUE
           ======================================================== */

        if (
            request.Details
                .Select(
                    detail =>
                        detail.LineSeq)
                .Distinct()
                .Count()
            !=
            request.Details.Count)
        {
            throw new ArgumentException(
                "Fuel line sequence values must be unique.");
        }


        /* ========================================================
           PO NO + PO LINE MUST BE UNIQUE

           Important for multi-PO:

               PO 066239 / Line 1
               PO 066216 / Line 1

           are NOT duplicates.
           ======================================================== */

        var duplicatePoLine =
            request.Details

                .GroupBy(
                    detail =>
                        new
                        {
                            PoNo =
                                Clean(
                                    detail.PoNo)
                                ?.ToUpperInvariant()
                                ?? string.Empty,

                            PoLineNo =
                                detail.PoLineNo
                        })

                .FirstOrDefault(
                    group =>
                        group.Count() > 1);


        if (duplicatePoLine is not null)
        {
            throw new ArgumentException(
                "The same Purchase Order line cannot be added more than once.");
        }


        /* ========================================================
           DETAIL VALIDATION
           ======================================================== */

        foreach (
            var detail
            in request.Details)
        {
            /* ----------------------------------------------------
               PO SOURCE
               ---------------------------------------------------- */

            if (
                string.IsNullOrWhiteSpace(
                    detail.PoNo)

                ||
                !detail.PoLineNo.HasValue

                ||
                detail.PoLineNo.Value <= 0)
            {
                throw new ArgumentException(
                    $"PO No. and PO Line are required on line {detail.LineSeq}.");
            }


            if (
                string.IsNullOrWhiteSpace(
                    detail.ItemCode)

                ||
                string.IsNullOrWhiteSpace(
                    detail.ItemDesc)

                ||
                !detail.QtyPo.HasValue

                ||
                detail.QtyPo.Value <= 0

                ||
                string.IsNullOrWhiteSpace(
                    detail.PoUm))
            {
                throw new ArgumentException(
                    $"Item Code, Description, PO Qty, and PO U/M are required on line {detail.LineSeq}.");
            }


            /* ----------------------------------------------------
               INTERNAL FUEL VALUES
               ---------------------------------------------------- */

            if (
                detail.LineSeq <= 0

                ||
                detail.FuelDate ==
                default

                ||
                string.IsNullOrWhiteSpace(
                    detail.FuelType)

                ||
                detail.Liters <= 0

                ||
                detail.UnitPrice <= 0)
            {
                throw new ArgumentException(
                    $"Line {detail.LineSeq} requires Fuel Date, Fuel Type, positive quantity, and positive Unit Price.");
            }
        }
    }


    /* ============================================================
       HELPERS
       ============================================================ */

    private static object Db(
        string? value) =>
        Clean(value) is { } cleaned
            ? cleaned
            : DBNull.Value;


    private static string? Clean(
        string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}