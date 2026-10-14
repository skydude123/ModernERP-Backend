using HRIStwinbeeAPI.DTO.AssetAcceptance;
using HRIStwinbeeAPI.DTO.Depreciation;

namespace HRIStwinbeeAPI.Services.Depreciation;

public interface IDepreciationService
{
    Task<PagedResultDto<DepreciationRunListItemDto>> QueryAsync(DepreciationRunQueryDto request, CancellationToken token);
    Task<DepreciationRunDto?> GetAsync(string depRunNo, CancellationToken token);
    Task<DepreciationPreviewResponseDto> PreviewAsync(DepreciationPreviewRequestDto request, CancellationToken token);
    Task<DepreciationCommandResultDto> SaveAsync(DepreciationSaveRequestDto request, CancellationToken token);
    Task<DepreciationCommandResultDto> TransitionAsync(string depRunNo, string action, DepreciationCommandRequestDto request, CancellationToken token);
    Task<DepreciationCommandResultDto> ReverseAsync(string depRunNo, DepreciationReverseRequestDto request, CancellationToken token);
    Task<IReadOnlyList<DepreciationLookupDto>> GetLookupAsync(string procedure, string? search, CancellationToken token);
    Task<IReadOnlyList<DepreciationPreviewLineDto>> GetEligibleAssetsAsync(DepreciationPreviewRequestDto request, CancellationToken token);
    Task<IReadOnlyList<DepreciationAssetHistoryDto>> GetAssetHistoryAsync(long assetId, CancellationToken token);
}
