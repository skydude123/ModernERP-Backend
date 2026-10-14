namespace HRIStwinbeeAPI.DTO
{
    public class BiometrixDtrListingUpdateDto
    {
        public long DtrId { get; set; }
        public DateTime WorkDate { get; set; }
        public string ScheduleStartTime { get; set; } = string.Empty;
        public string ScheduleEndTime { get; set; } = string.Empty;
        public string? TimeIn { get; set; }
        public string? TimeOut { get; set; }
        public decimal NoOfHoursWork { get; set; }
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
