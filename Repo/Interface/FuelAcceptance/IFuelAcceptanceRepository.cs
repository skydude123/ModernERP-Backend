using HRIStwinbeeAPI.DTO.FuelAcceptance;

namespace HRIStwinbeeAPI.Repo.Interface.FuelAcceptance;

public interface IFuelAcceptanceRepository
{
    Task<FuelPagedResultDto<FuelAcceptanceListItemDto>> QueryAsync(
        FuelAcceptanceQueryDto query,
        CancellationToken cancellationToken);


    Task<FuelAcceptanceDocumentDto?> GetAsync(
        string fuelAcceptanceNo,
        CancellationToken cancellationToken);


    Task<FuelAcceptanceCommandResultDto> SaveAsync(
        FuelAcceptanceSaveDto request,
        string userId,
        CancellationToken cancellationToken);


    Task<FuelAcceptanceCommandResultDto> PostAsync(
        string fuelAcceptanceNo,
        string rowVersion,
        string userId,
        CancellationToken cancellationToken);


    Task<FuelAcceptanceCommandResultDto> ReverseAsync(
        string fuelAcceptanceNo,
        string rowVersion,
        string reason,
        string userId,
        CancellationToken cancellationToken);


    /* ============================================================
       NEW:
       Supplier-based open PO line lookup.

       One supplier can return lines from many PO numbers.
       ============================================================ */

    Task<IReadOnlyList<FuelPoLineLookupDto>> GetOpenPoLinesAsync(
        string supplierCode,
        string? excludeAcceptanceNo,
        string? search,
        CancellationToken cancellationToken);


    Task<IReadOnlyList<FuelSupplierLookupDto>> GetSuppliersAsync(
        string? search,
        CancellationToken cancellationToken);


    Task<IReadOnlyList<FuelBranchLookupDto>> GetBranchesAsync(
        string? search,
        CancellationToken cancellationToken);


    Task<IReadOnlyList<FuelTaxSchemeLookupDto>> GetTaxSchemesAsync(
        CancellationToken cancellationToken);


    Task<IReadOnlyList<FuelVpSourceDto>> GetOpenForVpAsync(
        string? supplierCode,
        string? branchName,
        string? search,
        CancellationToken cancellationToken);
}