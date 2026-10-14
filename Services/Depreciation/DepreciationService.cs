using HRIStwinbeeAPI.DTO.AssetAcceptance;
using HRIStwinbeeAPI.DTO.Depreciation;
using HRIStwinbeeAPI.Repo.Interface.Depreciation;
using HRIStwinbeeAPI.Services.Suppliers;

namespace HRIStwinbeeAPI.Services.Depreciation;

public sealed class DepreciationService(
    IDepreciationRepository repository,
    ICurrentUserContext currentUser) : IDepreciationService
{
    private static readonly HashSet<string> Actions =
        new(StringComparer.OrdinalIgnoreCase)
        { "submit", "approve", "post", "delete", "cancel" };

    public Task<PagedResultDto<DepreciationRunListItemDto>> QueryAsync(DepreciationRunQueryDto request, CancellationToken token)
        => repository.QueryAsync(request, token);

    public Task<DepreciationRunDto?> GetAsync(string depRunNo, CancellationToken token)
        => repository.GetAsync(RequiredNo(depRunNo), token);

    public Task<DepreciationPreviewResponseDto> PreviewAsync(DepreciationPreviewRequestDto request, CancellationToken token)
    {
        ValidatePeriod(request.PeriodKey);
        request.SelectedAssetIds = request.SelectedAssetIds.Distinct().Take(5000).ToList();
        return repository.PreviewAsync(request, token);
    }

    public Task<DepreciationCommandResultDto> SaveAsync(DepreciationSaveRequestDto request, CancellationToken token)
    {
        ValidatePeriod(request.Setup.PeriodKey);
        if (request.Lines.Count > 5000)
            throw new ArgumentException("A depreciation run cannot contain more than 5,000 asset lines.");
        if (request.Lines.Select(x => x.AssetId).Distinct().Count() != request.Lines.Count)
            throw new ArgumentException("Duplicate asset lines are not allowed.");
        return repository.SaveAsync(request, currentUser.GetRequiredUserId(), token);
    }

    public Task<DepreciationCommandResultDto> TransitionAsync(
        string depRunNo, string action, DepreciationCommandRequestDto request, CancellationToken token)
    {
        if (!Actions.Contains(action))
            throw new ArgumentException("Unsupported depreciation workflow action.", nameof(action));
        return repository.TransitionAsync(
            RequiredNo(depRunNo), action.ToUpperInvariant(), request,
            currentUser.GetRequiredUserId(), token);
    }

    public Task<DepreciationCommandResultDto> ReverseAsync(
        string depRunNo, DepreciationReverseRequestDto request, CancellationToken token)
        => repository.ReverseAsync(
            RequiredNo(depRunNo), request, currentUser.GetRequiredUserId(), token);

    public Task<IReadOnlyList<DepreciationLookupDto>> GetLookupAsync(
        string procedure, string? search, CancellationToken token)
        => repository.GetLookupAsync(procedure, search, token);

    public Task<IReadOnlyList<DepreciationPreviewLineDto>> GetEligibleAssetsAsync(
        DepreciationPreviewRequestDto request, CancellationToken token)
    {
        ValidatePeriod(request.PeriodKey);
        return repository.GetEligibleAssetsAsync(request, token);
    }

    public Task<IReadOnlyList<DepreciationAssetHistoryDto>> GetAssetHistoryAsync(long assetId, CancellationToken token)
        => assetId > 0
            ? repository.GetAssetHistoryAsync(assetId, token)
            : throw new ArgumentOutOfRangeException(nameof(assetId));

    private static string RequiredNo(string value)
        => !string.IsNullOrWhiteSpace(value) && value.Length <= 30
            ? value.Trim()
            : throw new ArgumentException("A valid depreciation run number is required.");

    private static void ValidatePeriod(string periodKey)
    {
        if (periodKey.Length != 6 ||
            !int.TryParse(periodKey[..4], out var year) ||
            !int.TryParse(periodKey[4..], out var month) ||
            year is < 1900 or > 9999 || month is < 1 or > 12)
            throw new ArgumentException("Period must use YYYYMM format.");
    }
}
