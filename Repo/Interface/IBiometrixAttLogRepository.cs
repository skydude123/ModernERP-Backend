using HRIStwinbeeAPI.DTO;

namespace HRIStwinbeeAPI.Repo.Interface
{
    public interface IBiometrixAttLogRepository
    {
        Task<long> SaveUploadBatchAsync(BiometrixSaveUploadBatchRequestDto request, CancellationToken cancellationToken = default);
        Task ProcessUploadBatchAsync(long uploadBatchId, CancellationToken cancellationToken = default);
        Task<BiometrixUploadBatchDetailResponseDto?> GetBatchByIdAsync(long uploadBatchId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<BiometrixUploadBatchHistoryResponseDto>> GetHistoryAsync(int take, CancellationToken cancellationToken = default);


        Task<IReadOnlyList<BiometrixPayrollPeriodDto>> GetPayrollPeriodsAsync(
    CancellationToken cancellationToken);

        Task<IReadOnlyList<BiometrixBranchDto>> GetBranchesAsync(
            CancellationToken cancellationToken);

        Task<BiometrixAttendanceOtUpdateResponseDto> UpdateAttendanceOtAsync(
            BiometrixAttendanceOtUpdateRequestDto request,
            CancellationToken cancellationToken);
    }
}
