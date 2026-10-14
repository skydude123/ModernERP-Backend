using HRIStwinbeeAPI.DTO;

namespace HRIStwinbeeAPI.Repo.Interface
{
    public interface IBiometrixAttlogEditRepository
    {
        Task<IReadOnlyList<BiometrixPayrollPeriodDto>> GetPayrollPeriodsAsync(
           CancellationToken cancellationToken);

        Task<IReadOnlyList<BiometrixBranchDto>> GetBranchesAsync(
            CancellationToken cancellationToken);

        Task<BiometrixAttendanceOtUpdateResponseDto> UpdateAttendanceOtAsync(
            BiometrixAttendanceOtUpdateRequestDto request,
            CancellationToken cancellationToken);
    }
}
