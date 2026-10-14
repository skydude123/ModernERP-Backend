using HRIStwinbeeAPI.DTO.AssetAcceptance;


namespace HRIStwinbeeAPI.Repo.Interface.AssetAcceptance;

public interface IAssetAcceptanceRepository
{
    //Task<PagedResultDto<AssetAcceptanceListItemDto>> QueryAsync(AssetAcceptanceQueryDto request, CancellationToken cancellationToken);
    //Task<AssetAcceptanceDocumentDto?> GetAsync(string documentNo, CancellationToken cancellationToken);
    //Task<AssetAcceptanceCommandResultDto> UpsertAsync(AssetAcceptanceSaveDto request, CancellationToken cancellationToken);
    //Task<AssetAcceptanceCommandResultDto> TransitionAsync(string documentNo, string action, WorkflowRequestDto request, CancellationToken cancellationToken);
    //Task<AssetAcceptanceCommandResultDto> DeleteAsync(string documentNo, WorkflowRequestDto request, CancellationToken cancellationToken);
    //Task<IReadOnlyList<FixedAssetPoLookupDto>> GetFixedAssetPosAsync(string? search, CancellationToken cancellationToken);
    //Task<IReadOnlyList<FixedAssetPoLineLookupDto>> GetFixedAssetPoLinesAsync(string poNo, CancellationToken cancellationToken);
    //Task<IReadOnlyList<SupplierLookupDto>> GetSuppliersAsync(string? search, CancellationToken cancellationToken);
    //Task<IReadOnlyList<BranchLookupDto>> GetBranchesAsync(string? search, CancellationToken cancellationToken);
    //Task<IReadOnlyList<CodeNameLookupDto>> GetCodeLookupAsync(string procedure, string? search, CancellationToken cancellationToken);
    Task<PagedResultDto<AssetAcceptanceListItemDto>> QueryAsync(
            AssetAcceptanceQueryDto request,
            CancellationToken cancellationToken);

    Task<AssetAcceptanceDocumentDto?> GetAsync(
        string documentNo,
        CancellationToken cancellationToken);

    Task<AssetAcceptanceCommandResultDto> UpsertAsync(
        AssetAcceptanceSaveDto request,
        CancellationToken cancellationToken);

    Task<AssetAcceptanceCommandResultDto> TransitionAsync(
        string documentNo,
        string action,
        WorkflowRequestDto request,
        CancellationToken cancellationToken);

    Task<AssetAcceptanceCommandResultDto> DeleteAsync(
        string documentNo,
        WorkflowRequestDto request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<FixedAssetPoLookupDto>> GetFixedAssetPosAsync(
        string? search,
        string? excludeAcceptanceNo,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<FixedAssetPoLineLookupDto>>
        GetFixedAssetPoLinesAsync(
            string poNo,
            string? excludeAcceptanceNo,
            CancellationToken cancellationToken);

    Task<IReadOnlyList<SupplierLookupDto>> GetSuppliersAsync(
        string? search,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<BranchLookupDto>> GetBranchesAsync(
        string? search,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<CodeNameLookupDto>> GetCodeLookupAsync(
        string procedure,
        string? search,
        CancellationToken cancellationToken);

    Task<
         IReadOnlyList<
             AssetAccountLookupDto>>
         GetAccountsAsync(
             string purpose,
             string? search,
             int take,
             CancellationToken cancellationToken);


}
