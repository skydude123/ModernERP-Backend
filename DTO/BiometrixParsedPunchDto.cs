namespace HRIStwinbeeAPI.DTO
{
    public class BiometrixParsedPunchDto
    {
        public int SequenceNo { get; set; }
        public string? EmployeeIdNo { get; set; }
        public DateTime? PunchDateTime { get; set; }
        public string? RawColumn3 { get; set; }
        public string? RawColumn4 { get; set; }
        public string? RawColumn5 { get; set; }
        public string? RawColumn6 { get; set; }
        public string RawLine { get; set; } = string.Empty;
        public bool IsParserValid { get; set; }
        public string? ParserError { get; set; }
    }
}
