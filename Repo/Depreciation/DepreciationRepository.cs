using System.Data;
using Dapper;
using HRIStwinbeeAPI.ConnectionData;
using HRIStwinbeeAPI.DTO.AssetAcceptance;
using HRIStwinbeeAPI.DTO.Depreciation;
using HRIStwinbeeAPI.Repo.Interface.Depreciation;

namespace HRIStwinbeeAPI.Repo.Depreciation;

public sealed class DepreciationRepository(
    ISqlConnectionFactory connectionFactory) : IDepreciationRepository
{
    private static readonly HashSet<string> LookupProcedures =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "dbo.usp_Depreciation_GetBranches",
            "dbo.usp_Depreciation_GetAssetCategories",
            "dbo.usp_Depreciation_GetMethods",
            "dbo.usp_Depreciation_GetConventions",
            "dbo.usp_Depreciation_GetPostingAccounts"
        };

    public async Task<PagedResultDto<DepreciationRunListItemDto>> QueryAsync(
        DepreciationRunQueryDto request, CancellationToken token)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken: token);
        var rows = (await connection.QueryAsync<DepreciationRunListItemDto>(
            Command("dbo.usp_Depreciation_QueryRuns", new
            {
                period_from = Clean(request.PeriodFrom),
                period_to = Clean(request.PeriodTo),
                run_date_from = request.RunDateFrom,
                run_date_to = request.RunDateTo,
                status = request.Status,
                branch_code = Clean(request.BranchCode),
                asset_category_code = Clean(request.AssetCategoryCode),
                method_code = Clean(request.MethodCode),
                search = Clean(request.Search),
                skip = request.Skip,
                take = request.Take,
                sort_field = request.SortField,
                sort_dir = request.SortDir
            }, token))).AsList();
        return new PagedResultDto<DepreciationRunListItemDto>
        {
            Data = rows,
            Total = rows.FirstOrDefault()?.TotalCount ?? 0
        };
    }

    public async Task<DepreciationRunDto?> GetAsync(string depRunNo, CancellationToken token)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken: token);
        using var multi = await connection.QueryMultipleAsync(
            Command("dbo.usp_Depreciation_GetRunByNo", new { dep_run_no = depRunNo }, token));
        var header = await multi.ReadSingleOrDefaultAsync<DepreciationRunHeaderDto>();
        if (header is null) return null;
        return new DepreciationRunDto
        {
            Header = header,
            Details = (await multi.ReadAsync<DepreciationRunDetailDto>()).AsList(),
            GlLines = (await multi.ReadAsync<DepreciationGlPreviewLineDto>()).AsList(),
            Validations = (await multi.ReadAsync<DepreciationValidationDto>()).AsList()
        };
    }

    public async Task<DepreciationPreviewResponseDto> PreviewAsync(
        DepreciationPreviewRequestDto request, CancellationToken token)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken: token);
        using var multi = await connection.QueryMultipleAsync(
            Command("dbo.usp_Depreciation_Preview", PreviewParameters(request), token));
        var lines = (await multi.ReadAsync<DepreciationPreviewLineDto>()).AsList();
        var gl = (await multi.ReadAsync<DepreciationGlPreviewLineDto>()).AsList();
        var validations = (await multi.ReadAsync<DepreciationValidationDto>()).AsList();
        var selected = lines.Where(x => x.IsSelected && x.EligibilityStatus == "ELIGIBLE").ToList();
        return new DepreciationPreviewResponseDto
        {
            Lines = lines,
            GlLines = gl,
            Validations = validations,
            EligibleAssetCount = lines.Count(x => x.EligibilityStatus == "ELIGIBLE"),
            SelectedAssetCount = selected.Count,
            TotalAcquisitionCost = selected.Sum(x => x.AcquisitionCost),
            TotalOpeningAccumulatedDepreciation = selected.Sum(x => x.OpeningAccumulatedDepreciation),
            TotalCurrentDepreciation = selected.Sum(x => x.CurrentDepreciation),
            TotalClosingAccumulatedDepreciation = selected.Sum(x => x.ClosingAccumulatedDepreciation),
            TotalClosingNetBookValue = selected.Sum(x => x.ClosingNetBookValue),
            GlDifference = gl.Sum(x => x.Debit - x.Credit)
        };
    }

    public async Task<DepreciationCommandResultDto> SaveAsync(
        DepreciationSaveRequestDto request, string userId, CancellationToken token)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken: token);
        return await connection.QuerySingleAsync<DepreciationCommandResultDto>(
            Command("dbo.usp_Depreciation_SaveDraft", new
            {
                dep_run_no = Clean(request.DepRunNo),
                period_key = request.Setup.PeriodKey,
                run_date = request.RunDate.Date,
                branch_code = Clean(request.Setup.BranchCode),
                asset_category_code = Clean(request.Setup.AssetCategoryCode),
                method_code = Clean(request.Setup.MethodCode),
                convention = request.Setup.Convention,
                include_zero = request.Setup.IncludeZeroAmount,
                remarks = Clean(request.Remarks),
                user_id = userId,
                expected_row_version = CleanRowVersion(request.RowVersion),
                details = BuildDetails(request.Lines)
            }, token));
    }

    public async Task<DepreciationCommandResultDto> TransitionAsync(
        string depRunNo, string action, DepreciationCommandRequestDto request,
        string userId, CancellationToken token)
    {
        var procedure = action switch
        {
            "SUBMIT" => "dbo.usp_Depreciation_Submit",
            "APPROVE" => "dbo.usp_Depreciation_Approve",
            "POST" => "dbo.usp_Depreciation_Post",
            "DELETE" => "dbo.usp_Depreciation_DeleteDraft",
            "CANCEL" => "dbo.usp_Depreciation_CancelDraft",
            _ => throw new ArgumentOutOfRangeException(nameof(action))
        };
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken: token);
        return await connection.QuerySingleAsync<DepreciationCommandResultDto>(
            Command(procedure, new
            {
                dep_run_no = depRunNo,
                expected_row_version = CleanRowVersion(request.RowVersion),
                user_id = userId,
                reason = Clean(request.Reason)
            }, token));
    }

    public async Task<DepreciationCommandResultDto> ReverseAsync(
        string depRunNo, DepreciationReverseRequestDto request,
        string userId, CancellationToken token)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken: token);
        return await connection.QuerySingleAsync<DepreciationCommandResultDto>(
            Command("dbo.usp_Depreciation_Reverse", new
            {
                dep_run_no = depRunNo,
                expected_row_version = CleanRowVersion(request.RowVersion),
                user_id = userId,
                reason = request.Reason.Trim()
            }, token));
    }

    public async Task<IReadOnlyList<DepreciationLookupDto>> GetLookupAsync(
        string procedure, string? search, CancellationToken token)
    {
        if (!LookupProcedures.Contains(procedure))
            throw new ArgumentException("Unsupported depreciation lookup.", nameof(procedure));
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken: token);
        return (await connection.QueryAsync<DepreciationLookupDto>(
            Command(procedure, new { search = Clean(search) }, token))).AsList();
    }

    public async Task<IReadOnlyList<DepreciationPreviewLineDto>> GetEligibleAssetsAsync(
        DepreciationPreviewRequestDto request, CancellationToken token)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken: token);
        return (await connection.QueryAsync<DepreciationPreviewLineDto>(
            Command("dbo.usp_Depreciation_GetEligibleAssets", PreviewParameters(request), token))).AsList();
    }

    public async Task<IReadOnlyList<DepreciationAssetHistoryDto>> GetAssetHistoryAsync(
        long assetId, CancellationToken token)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken: token);
        return (await connection.QueryAsync<DepreciationAssetHistoryDto>(
            Command("dbo.usp_Depreciation_GetAssetHistory", new { asset_id = assetId }, token))).AsList();
    }

    private static object PreviewParameters(DepreciationPreviewRequestDto request) => new
    {
        period_key = request.PeriodKey,
        branch_code = Clean(request.BranchCode),
        asset_category_code = Clean(request.AssetCategoryCode),
        method_code = Clean(request.MethodCode),
        convention = request.Convention,
        include_zero = request.IncludeZeroAmount,
        selected_asset_ids = BuildAssetIds(request.SelectedAssetIds)
    };

    private static SqlMapper.ICustomQueryParameter BuildAssetIds(IEnumerable<long> ids)
    {
        var table = new DataTable();
        table.Columns.Add("asset_id", typeof(long));
        foreach (var id in ids.Distinct()) table.Rows.Add(id);
        return table.AsTableValuedParameter("dbo.DepreciationAssetIdType");
    }

    private static SqlMapper.ICustomQueryParameter BuildDetails(IEnumerable<DepreciationPreviewLineDto> lines)
    {
        var table = new DataTable();
        table.Columns.Add("line_no", typeof(int));
        table.Columns.Add("asset_id", typeof(long));
        table.Columns.Add("is_selected", typeof(bool));
        foreach (var line in lines) table.Rows.Add(line.LineNo, line.AssetId, line.IsSelected);
        return table.AsTableValuedParameter("dbo.DepreciationRunDetailType");
    }



    private static CommandDefinition Command(string procedure, object? parameters, CancellationToken token)
        => new(procedure, parameters, commandType: CommandType.StoredProcedure, cancellationToken: token);

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();



    private static string? CleanRowVersion(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var result = value.Trim();

        if (result.StartsWith(
            "0x",
            StringComparison.OrdinalIgnoreCase))
        {
            result = result[2..];
        }

        return result.ToUpperInvariant();
    }
}
