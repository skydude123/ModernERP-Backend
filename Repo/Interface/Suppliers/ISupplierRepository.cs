using HRIStwinbeeAPI.DTO.Suppliers;

namespace HRIStwinbeeAPI.Repo.Interface.Suppliers;

public interface ISupplierRepository
{
    Task<SupplierPagedResult<SupplierListItemDto>> QueryAsync(
        SupplierListQueryRequest request,
        CancellationToken cancellationToken);

    Task<SupplierDetailDto?> GetByCodeAsync(
        string supplierCode,
        CancellationToken cancellationToken);

    Task<SupplierValidationResultDto> ValidateAsync(
        string supplierCode,
        SupplierWriteRequest request,
        bool isCreate,
        CancellationToken cancellationToken);

    Task<SupplierCommandResultDto> CreateAsync(SupplierCreateRequest request,CancellationToken cancellationToken);

    Task<SupplierCommandResultDto> UpdateAsync(
        SupplierUpdateRequest request,
        //string userId,
        CancellationToken cancellationToken);

    Task<SupplierCommandResultDto> SetActiveStatusAsync(
        string supplierCode,
        string activeFlag,
      //  DateTime expectedModifiedDate,
        string userId,
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
