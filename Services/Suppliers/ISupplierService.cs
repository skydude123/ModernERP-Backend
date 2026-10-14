using HRIStwinbeeAPI.DTO.Suppliers;

namespace HRIStwinbeeAPI.Services.Suppliers;

public interface ISupplierService
{
    Task<SupplierPagedResult<SupplierListItemDto>> QueryAsync(
        SupplierListQueryRequest request,
        CancellationToken cancellationToken);

    Task<SupplierDetailDto?> GetByCodeAsync(
        string supplierCode,
        CancellationToken cancellationToken);

    Task<SupplierCommandResultDto> CreateAsync(
        SupplierCreateRequest request,
        CancellationToken cancellationToken);

    Task<SupplierCommandResultDto> UpdateAsync(
        string supplierCode,
        SupplierUpdateRequest request,
        CancellationToken cancellationToken);

    Task<SupplierCommandResultDto> SetActiveStatusAsync(
        string supplierCode,
        string activeFlag,
        //SupplierStatusRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<SupplierLookupDto>> GetGroupsAsync(
        string? search,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<SupplierLookupDto>> GetTermsAsync(
        string? search,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<SupplierLookupDto>> GetCurrenciesAsync(
        string? search,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<SupplierLookupDto>> GetTaxProfilesAsync(
        string? search,
        CancellationToken cancellationToken);
}
