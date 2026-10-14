using Dapper;
using HRIStwinbeeAPI.ConnectionData;
using HRIStwinbeeAPI.DTO.AssetAcceptance;
using HRIStwinbeeAPI.Repo.Interface.AssetAcceptance;
using System.Linq;
using System.Data;
//using System.Data.SqlClient;
using Microsoft.Data.SqlClient;

namespace HRIStwinbeeAPI.Repo.AssetAcceptance;

public sealed class AssetAcceptanceRepository(
    ISqlConnectionFactory connectionFactory)
    : IAssetAcceptanceRepository
{
    private static readonly HashSet<string> LookupProcedures =
    [
        "dbo.usp_AssetAcceptance_GetCategories",
        "dbo.usp_AssetAcceptance_GetDepreciationMethods",
        "dbo.usp_AssetAcceptance_GetLocations",
        "dbo.usp_AssetAcceptance_GetCustodians",
        //"dbo.usp_AssetAcceptance_GetAccounts"
    ];

    public async Task<PagedResultDto<AssetAcceptanceListItemDto>> QueryAsync(
        AssetAcceptanceQueryDto request,
        CancellationToken cancellationToken)
    {
        await using var connection =
            await connectionFactory.CreateOpenConnectionAsync(
                cancellationToken: cancellationToken);

        var rows = (
            await connection.QueryAsync<AssetAcceptanceListItemDto>(
                Command(
                    "dbo.usp_AssetAcceptance_List",
                    new
                    {
                        from_date = request.FromDate,
                        to_date = request.ToDate,
                        status = NormalizeStatus(request.Status),
                        branch_name = Clean(request.BranchName),
                        asset_category_code =
                            Clean(request.AssetCategoryCode),
                        search = Clean(request.Search),
                        skip = Math.Max(0, request.Skip),
                        take = Math.Clamp(request.Take, 1, 200),
                        sort_field = Clean(request.SortField)
                            ?? "acceptanceDate",
                        sort_dir =
                            NormalizeSortDirection(request.SortDir)
                    },
                    cancellationToken)))
            .AsList();

        return new PagedResultDto<AssetAcceptanceListItemDto>
        {
            Data = rows,
            Total = rows.FirstOrDefault()?.TotalCount ?? 0
        };
    }

    public async Task<AssetAcceptanceDocumentDto?> GetAsync(
        string documentNo,
        CancellationToken cancellationToken)
    {
        await using var connection =
            await connectionFactory.CreateOpenConnectionAsync(
                cancellationToken: cancellationToken);

        using var multi =
            await connection.QueryMultipleAsync(
                Command(
                    "dbo.usp_AssetAcceptance_Get",
                    new
                    {
                        asset_acceptance_no =
                            documentNo.Trim()
                    },
                    cancellationToken));

        var header =
            await multi.ReadSingleOrDefaultAsync<
                AssetAcceptanceHeaderDto>();

        if (header is null)
        {
            return null;
        }

        var details =
            (await multi.ReadAsync<AssetAcceptanceDetailDto>())
            .AsList();

        return new AssetAcceptanceDocumentDto
        {
            Header = header,
            Details = details
        };
    }

    public async Task<AssetAcceptanceCommandResultDto> UpsertAsync(
     AssetAcceptanceSaveDto request,
     CancellationToken cancellationToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.Header);
            ArgumentNullException.ThrowIfNull(request.Details);

            var header = request.Header;

            var parameters = new DynamicParameters(
                new
                {
                    asset_acceptance_no =
                        Clean(header.AssetAcceptanceNo),

                    acceptance_date =
                        header.AcceptanceDate,

                    po_no =
                        header.PoNo.Trim(),

                    supp_code =
                        header.SuppCode.Trim(),

                    asset_category_code =
                        header.AssetCategoryCode.Trim(),

                    branch_name =
                        header.BranchName.Trim(),

                    department =
                        header.Department.Trim(),

                    received_by =
                        header.ReceivedBy.Trim(),

                    accepted_by =
                        header.AcceptedBy.Trim(),

                    in_service_date =
                        header.InServiceDate,

                    asset_location_code =
                        header.AssetLocationCode.Trim(),

                    acquisition_mode =
                        header.AcquisitionMode
                            .Trim()
                            .ToUpperInvariant(),

                    downpayment_amount =
                        header.DownpaymentAmount,

                    financed_amount =
                        header.FinancedAmount,

                    financing_company =
                        Clean(header.FinancingCompany),

                    loan_reference =
                        Clean(header.LoanReference),

                    remarks =
                        Clean(header.Remarks),

                    user_id =
                        header.UserId.Trim(),

                    expected_row_version =
                        Clean(header.RowVersion)
                });

            parameters.Add(
                "details",
                BuildDetails(request.Details));

            await using var connection =
                await connectionFactory.CreateOpenConnectionAsync(
                    cancellationToken: cancellationToken);

            var result =
                await connection.QuerySingleAsync<AssetAcceptanceCommandResultDto>(
                    Command(
                        "dbo.usp_AssetAcceptance_Upsert",
                        parameters,
                        cancellationToken));

            return result;
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            // Ayaw i-convert ngadto sa normal error.
            // Cancellation request ni gikan sa client/request pipeline.
            throw;
        }
        catch (SqlException ex)
        {
            /*
             * Apil dinhi ang:
             * - SQL THROW errors gikan sa stored procedure
             * - Constraint violation
             * - Connection error
             * - Timeout
             * - Deadlock
             */

            throw new InvalidOperationException(
                $"Database error while saving Asset Acceptance: {ex.Message}",
                ex);
        }
        catch (InvalidOperationException ex)
        {
            /*
             * Posibleng gikan sa QuerySingleAsync kung:
             * - Walay result gi-return ang stored procedure
             * - More than one result row ang gi-return
             */

            throw new InvalidOperationException(
                $"The Asset Acceptance save operation returned an invalid result: {ex.Message}",
                ex);
        }
        catch (ArgumentException)
        {
            // Validation/programming error.
            // I-preserve ang original exception ug stack trace.
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"An unexpected error occurred while saving Asset Acceptance: {ex.Message}",
                ex);
        }
    }

    //public async Task<AssetAcceptanceCommandResultDto> TransitionAsync(
    //    string documentNo,
    //    string action,
    //    WorkflowRequestDto request,
    //    CancellationToken cancellationToken)
    //{
    //    await using var connection =
    //        await connectionFactory.CreateOpenConnectionAsync(
    //            cancellationToken: cancellationToken);

    //    return await connection
    //        .QuerySingleAsync<AssetAcceptanceCommandResultDto>(
    //            Command(
    //                "dbo.usp_AssetAcceptance_Transition",
    //                new
    //                {
    //                    asset_acceptance_no =
    //                        documentNo.Trim(),

    //                    action =
    //                        action.Trim().ToUpperInvariant(),

    //                    user_id =
    //                        request.UserId.Trim(),

    //                    expected_row_version =
    //                        Clean(request.RowVersion),

    //                    reason =
    //                        Clean(request.Reason)
    //                },
    //                cancellationToken));
    //}

    public async Task<AssetAcceptanceCommandResultDto> TransitionAsync(
    string documentNo,
    string action,
    WorkflowRequestDto request,
    CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(documentNo))
            {
                throw new ArgumentException(
                    "Asset Acceptance Number is required.",
                    nameof(documentNo));
            }

            if (string.IsNullOrWhiteSpace(action))
            {
                throw new ArgumentException(
                    "Workflow action is required.",
                    nameof(action));
            }

            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                throw new ArgumentException(
                    "User ID is required.",
                    nameof(request.UserId));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var parameters = new
            {
                asset_acceptance_no =
                    documentNo.Trim(),

                action =
                    action.Trim().ToUpperInvariant(),

                user_id =
                    request.UserId.Trim(),

                expected_row_version =
                    Clean(request.RowVersion),

                reason =
                    Clean(request.Reason)
            };

            await using var connection =
                await connectionFactory.CreateOpenConnectionAsync(
                    cancellationToken: cancellationToken);

            var result =
                await connection
                    .QuerySingleAsync<AssetAcceptanceCommandResultDto>(
                        Command(
                            "dbo.usp_AssetAcceptance_Transition",
                            parameters,
                            cancellationToken));

            return result;
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            // Request was cancelled by the client or application.
            // Preserve the original cancellation exception.
            throw;
        }
        catch (SqlException ex)
        {
            /*
             * Possible SQL errors:
             * - Errors raised using THROW in the stored procedure
             * - Row-version or concurrency conflict
             * - Invalid workflow transition
             * - SQL timeout
             * - Deadlock
             * - Connection error
             */

            throw new InvalidOperationException(
                $"Database error while processing Asset Acceptance transition: {ex.Message}",
                ex);
        }
        catch (ArgumentException)
        {
            // Preserve validation errors and their original stack trace.
            throw;
        }
        catch (InvalidOperationException ex)
        {
            /*
             * QuerySingleAsync can throw InvalidOperationException when:
             * - The stored procedure returns no result row
             * - The stored procedure returns more than one result row
             */

            throw new InvalidOperationException(
                $"The Asset Acceptance transition returned an invalid result: {ex.Message}",
                ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"An unexpected error occurred while processing the Asset Acceptance transition: {ex.Message}",
                ex);
        }
    }

    public async Task<AssetAcceptanceCommandResultDto> DeleteAsync(
        string documentNo,
        WorkflowRequestDto request,
        CancellationToken cancellationToken)
    {
        await using var connection =
            await connectionFactory.CreateOpenConnectionAsync(
                cancellationToken: cancellationToken);

        return await connection
            .QuerySingleAsync<AssetAcceptanceCommandResultDto>(
                Command(
                    "dbo.usp_AssetAcceptance_Delete",
                    new
                    {
                        asset_acceptance_no =
                            documentNo.Trim(),

                        user_id =
                            request.UserId.Trim(),

                        expected_row_version =
                            Clean(request.RowVersion)
                    },
                    cancellationToken));
    }

    public Task<IReadOnlyList<FixedAssetPoLookupDto>>
        GetFixedAssetPosAsync(
            string? search,
            string? excludeAcceptanceNo,
            CancellationToken cancellationToken)
    {
        return QueryLookupAsync<FixedAssetPoLookupDto>(
            "dbo.usp_AssetAcceptance_GetFixedAssetPos",
            new
            {
                search = Clean(search),

                exclude_acceptance_no =
                    Clean(excludeAcceptanceNo)
            },
            cancellationToken);
    }

    public Task<IReadOnlyList<FixedAssetPoLineLookupDto>>
        GetFixedAssetPoLinesAsync(
            string poNo,
            string? excludeAcceptanceNo,
            CancellationToken cancellationToken)
    {
        return QueryLookupAsync<FixedAssetPoLineLookupDto>(
            "dbo.usp_AssetAcceptance_GetFixedAssetPoLines",
            new
            {
                po_no = poNo.Trim(),

                exclude_acceptance_no =
                    Clean(excludeAcceptanceNo)
            },
            cancellationToken);
    }

    public Task<IReadOnlyList<SupplierLookupDto>> GetSuppliersAsync(
        string? search,
        CancellationToken cancellationToken)
    {
        return QueryLookupAsync<SupplierLookupDto>(
            "dbo.usp_AssetAcceptance_GetSuppliers",
            new
            {
                search = Clean(search)
            },
            cancellationToken);
    }

    public Task<IReadOnlyList<BranchLookupDto>> GetBranchesAsync(
        string? search,
        CancellationToken cancellationToken)
    {
        return QueryLookupAsync<BranchLookupDto>(
            "dbo.usp_AssetAcceptance_GetBranches",
            new
            {
                search = Clean(search)
            },
            cancellationToken);
    }

    public Task<IReadOnlyList<CodeNameLookupDto>> GetCodeLookupAsync(
        string procedure,
        string? search,
        CancellationToken cancellationToken)
    {
        if (!LookupProcedures.Contains(procedure))
        {
            throw new ArgumentOutOfRangeException(
                nameof(procedure),
                procedure,
                "The requested lookup procedure is not allowed.");
        }

        return QueryLookupAsync<CodeNameLookupDto>(
            procedure,
            new
            {
                search = Clean(search)
            },
            cancellationToken);
    }

    private async Task<IReadOnlyList<T>> QueryLookupAsync<T>(
        string procedure,
        object parameters,
        CancellationToken cancellationToken)
    {
        await using var connection =
            await connectionFactory.CreateOpenConnectionAsync(
                cancellationToken: cancellationToken);

        return (
            await connection.QueryAsync<T>(
                Command(
                    procedure,
                    parameters,
                    cancellationToken)))
            .AsList();
    }

    private static CommandDefinition Command(
        string procedure,
        object? parameters,
        CancellationToken token)
    {
        return new CommandDefinition(
            commandText: procedure,
            parameters: parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: token);
    }

    private static string NormalizeStatus(string? value)
    {
        var status =
            Clean(value)?.ToUpperInvariant();

        return status is
            "DRAFT" or
            "SUBMITTED" or
            "ACCEPTED" or
            "POSTED" or
            "CANCELLED"
                ? status
                : "ALL";
    }

    private static string NormalizeSortDirection(string? value)
    {
        return string.Equals(
            value,
            "asc",
            StringComparison.OrdinalIgnoreCase)
                ? "asc"
                : "desc";
    }

    private static string? Clean(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static object Db(string? value)
    {
        return Clean(value) is { } text
            ? text
            : DBNull.Value;
    }

    private static SqlMapper.ICustomQueryParameter BuildDetails(
        IEnumerable<AssetAcceptanceDetailSaveDto> details)
    {
        var table = new DataTable();

        table.Columns.Add("line_no", typeof(int));
        table.Columns.Add("po_seq", typeof(int));
        table.Columns.Add("asset_code", typeof(string));
        table.Columns.Add("description", typeof(string));
        table.Columns.Add("qty", typeof(decimal));
        table.Columns.Add("unit_cost", typeof(decimal));
        table.Columns.Add("tracking_mode", typeof(string));
        table.Columns.Add("serial_no", typeof(string));
        table.Columns.Add("engine_no", typeof(string));
        table.Columns.Add("chassis_no", typeof(string));
        table.Columns.Add("plate_no", typeof(string));
        table.Columns.Add("warranty_info", typeof(string));
        table.Columns.Add("condition_status", typeof(string));
        table.Columns.Add("remarks", typeof(string));
        table.Columns.Add(
            "depreciation_method_code",
            typeof(string));
        table.Columns.Add(
            "useful_life_months",
            typeof(int));
        table.Columns.Add(
            "residual_value",
            typeof(decimal));
        table.Columns.Add(
            "depreciation_start_date",
            typeof(DateTime));
        table.Columns.Add(
            "asset_cost_account",
            typeof(string));
        table.Columns.Add(
            "depreciation_expense_account",
            typeof(string));
        table.Columns.Add(
            "accumulated_depreciation_account",
            typeof(string));
        table.Columns.Add(
            "custodian_code",
            typeof(string));
        table.Columns.Add(
            "asset_location_code",
            typeof(string));

        foreach (var detail in details.OrderBy(x => x.LineNo))
        {
            table.Rows.Add(
                detail.LineNo,
                detail.PoSeq,
                detail.AssetCode.Trim(),
                detail.Description.Trim(),
                detail.Qty,
                detail.UnitCost,
                detail.TrackingMode
                    .Trim()
                    .ToUpperInvariant(),
                Db(detail.SerialNo),
                Db(detail.EngineNo),
                Db(detail.ChassisNo),
                Db(detail.PlateNo),
                Db(detail.WarrantyInfo),
                detail.ConditionStatus
                    .Trim()
                    .ToUpperInvariant(),
                Db(detail.Remarks),
                detail.DepreciationMethodCode.Trim(),
                detail.UsefulLifeMonths,
                detail.ResidualValue,
                detail.DepreciationStartDate,
                detail.AssetCostAccount.Trim(),
                detail.DepreciationExpenseAccount.Trim(),
                detail.AccumulatedDepreciationAccount.Trim(),
                detail.CustodianCode.Trim(),
                detail.AssetLocationCode.Trim());
        }

        return table.AsTableValuedParameter(
            "dbo.AssetAcceptanceDetailBatchType");
    }


    public async Task<IReadOnlyList<AssetAccountLookupDto>>
      GetAccountsAsync(
          string purpose,
          string? search,
          int take,
          CancellationToken cancellationToken)
    {
        var normalizedPurpose =
            AssetAccountPurposes.Normalize(
                purpose);

        var procedureName =
            normalizedPurpose switch
            {
                AssetAccountPurposes.AssetCost =>
                    "dbo.sp_AssetLookup_AssetCostAccounts",

                AssetAccountPurposes.AccumulatedDepreciation =>
                    "dbo.sp_AssetLookup_AccumulatedDepreciationAccounts",

                AssetAccountPurposes.DepreciationExpense =>
                    "dbo.sp_AssetLookup_DepreciationExpenseAccounts",

                _ => throw new ArgumentOutOfRangeException(
                    nameof(purpose),
                    purpose,
                    "Unsupported asset account lookup purpose.")
            };

        await using var connection =
            await connectionFactory
                .CreateOpenConnectionAsync(
                    cancellationToken:
                        cancellationToken);

        var rows =
            (
                await connection.QueryAsync<
                    AssetAccountLookupDto>(
                    Command(
                        procedureName,
                        null,
                        cancellationToken))
            )
            .AsList();

        IEnumerable<AssetAccountLookupDto>
            filteredRows = rows;

        var normalizedSearch =
            Clean(search);

        if (normalizedSearch is not null)
        {
            filteredRows =
                filteredRows.Where(
                    account =>
                        account.AccountId.Contains(
                            normalizedSearch,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        account.AccountName.Contains(
                            normalizedSearch,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        account.DisplayText.Contains(
                            normalizedSearch,
                            StringComparison.OrdinalIgnoreCase));
        }

        var normalizedTake =
            Math.Clamp(
                take,
                1,
                500);

        return filteredRows
            .Take(normalizedTake)
            .ToList();
    }
}