namespace HRIStwinbeeAPI.DTO
{
    public class BiometrixDtrListingRowDto
    {
        public long DtrId { get; set; }
        public string DateFrom { get; set; } = string.Empty;
        public string DateTo { get; set; } = string.Empty;
        public long UploadBatchId { get; set; }
        public int PayrollPeriod { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string EmployeeIdNo { get; set; } = string.Empty;
        public string? HrisIdNo { get; set; }
        public string? EmpName { get; set; }
        public string? DayName { get; set; }
        public string WorkDate { get; set; } = string.Empty;
        public string? ScheduleStartTime { get; set; }
        public string? ScheduleEndTime { get; set; }
        public string? TimeIn { get; set; }
        public string? TimeOut { get; set; }
        public int NoOfHoursWorkMinutes { get; set; }
        public decimal NoOfHoursWork { get; set; }
        public int OtMinutes { get; set; }
        public decimal OtHours { get; set; }
        public string? Remarks { get; set; }
        public int? RawPunchCount { get; set; }
        public string? FirstPunchDateTime { get; set; }
        public string? LastPunchDateTime { get; set; }
        public decimal? Sot { get; set; }
        public decimal? Lot { get; set; }
        public decimal? Ut { get; set; }
    }
}
