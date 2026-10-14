using HRIStwinbeeAPI.DTO.MaterialIssuance;

namespace HRIStwinbeeAPI.Repo.Interface.MaterialIssuance;

public interface IMaterialIssuanceRepository
{
    Task<MaterialIssuancePagedResult<MaterialIssuanceListItemDto>> QueryAsync(MaterialIssuanceQueryRequest request, CancellationToken token);
    Task<MaterialIssuanceDocumentDto?> GetByNoAsync(string issueNo, CancellationToken token);
    Task<IReadOnlyList<MaterialIssuanceWarehouseDto>> GetWarehousesAsync(string? search, CancellationToken token);
    Task<IReadOnlyList<MaterialIssuanceSupplierDto>> GetSuppliersAsync(string? search, CancellationToken token);

    Task<IReadOnlyList<MaterialIssuanceSoVerificationDto>>
    VerifySoNoAsync(
        string soNo,
        CancellationToken token);

    Task<IReadOnlyList<MaterialLookupDto>> GetAvailableMaterialsAsync(string whseCode, string? search, CancellationToken token);
    Task<IReadOnlyList<FifoPreviewDto>> GetFifoPreviewAsync(string whseCode, string itemCode, decimal? baseQty, CancellationToken token);
    Task<MaterialIssuanceSaveResultDto> UpsertAsync(UpsertMaterialIssuanceRequest request, CancellationToken token);
    Task<MaterialIssuanceSaveResultDto> ReverseAsync(string issueNo, ReverseMaterialIssuanceRequest request, CancellationToken token);
    Task<IReadOnlyList<MaterialIssuanceWorkCenterDto>> GetWorkCentersAsync(
    string? search,
    CancellationToken token);
    //Task<IReadOnlyList<MaterialLookupDto>>
    // GetAvailableMaterialsAsync(
    //     string whseCode,
    //     string? search,
    //     CancellationToken token);
}
