namespace HRIStwinbeeAPI.DTO
{
    public sealed class BiometrixPayrollPeriodDto
    {
        public int PayrollPeriod { get; set; }

        public int FiscalYear { get; set; }

        public string FiscalPeriod { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;
    }

    public sealed class BiometrixBranchDto
    {
        public string BranchName { get; set; } = string.Empty;
    }

    public sealed class BiometrixAttendanceOtUpdateRequestDto
    {
        public long ProcessedAttendanceId { get; set; }

        public decimal OtHours { get; set; }

        public string? UpdatedBy { get; set; }
    }

    public sealed class BiometrixAttendanceOtUpdateResponseDto
    {
        public long ProcessedAttendanceId { get; set; }

        public long UploadBatchId { get; set; }

        public string EmployeeIdNo { get; set; } = string.Empty;

        public DateTime WorkDate { get; set; }

        public int OtMinutes { get; set; }

        public decimal OtHours { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string UpdatedBy { get; set; } = string.Empty;
    }
}
