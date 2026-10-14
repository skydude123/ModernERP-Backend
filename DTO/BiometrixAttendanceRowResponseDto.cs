namespace HRIStwinbeeAPI.DTO
{
    public class BiometrixAttendanceRowResponseDto
    {
        public string Idno { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string DayName { get; set; } = string.Empty;
        public string ScheduleCode { get; set; } = string.Empty;
        public string? TimeIn { get; set; }
        public string? TimeOut { get; set; }
        public decimal NoOfHoursWork { get; set; }
        public decimal Ot { get; set; }
        public string Remarks { get; set; } = string.Empty;
    }
}
