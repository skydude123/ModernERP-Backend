using HRIStwinbeeAPI.DTO.ServiceAcceptance;

namespace HRIStwinbeeAPI.Repo.Interface.ServiceAcceptance;

public interface IServiceAcceptanceRepository
{
    Task<
        ServiceAcceptancePagedResultDto<
            ServiceAcceptanceListItemDto>>
        QueryAsync(
            ServiceAcceptanceQueryDto query,
            CancellationToken token);


    Task<ServiceAcceptanceDocumentDto?>
        GetAsync(
            string documentNo,
            CancellationToken token);


    Task<ServiceAcceptanceCommandResultDto>
        SaveAsync(
            ServiceAcceptanceSaveDto request,
            CancellationToken token);


    Task<ServiceAcceptanceCommandResultDto>
        TransitionAsync(
            string documentNo,
            string action,
            ServiceAcceptanceWorkflowDto request,
            CancellationToken token);


    /*
     * NEW:
     * Supplier-driven open Service PO lines.
     */
    Task<
        IReadOnlyList<
            ServiceOpenPoLineLookupDto>>
        GetOpenPoLinesAsync(
            string supplierCode,
            string? branchName,
            string? excludeAcceptanceNo,
            string? search,
            CancellationToken token);


    Task<
        IReadOnlyList<
            ServiceSupplierLookupDto>>
        GetSuppliersAsync(
            string? search,
            CancellationToken token);


    Task<
        IReadOnlyList<
            ServiceBranchLookupDto>>
        GetBranchesAsync(
            CancellationToken token);


    Task<
        IReadOnlyList<
            ServiceTaxSchemeLookupDto>>
        GetTaxSchemesAsync(
            CancellationToken token);


    Task<
        IReadOnlyList<
            ServiceVpSourceDto>>
        GetVpSourcesAsync(
            string? supplierCode,
            string? branchName,
            string? search,
            CancellationToken token);
}