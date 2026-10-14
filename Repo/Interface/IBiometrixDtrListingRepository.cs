using HRIStwinbeeAPI.DTO;

namespace HRIStwinbeeAPI.Repo.Interface
{
    public interface IBiometrixDtrListingRepository
    {
        Task<IReadOnlyList<BiometrixLookupItemDto>> GetBranchesAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<BiometrixLookupItemDto>> GetPayrollPeriodsAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<BiometrixDtrListingRowDto>> GetListingAsync(BiometrixDtrListingFilterDto filter, CancellationToken cancellationToken = default);
        Task<BiometrixDtrListingRowDto?> UpdateAsync(BiometrixDtrListingUpdateDto request, CancellationToken cancellationToken = default);
    }
}
