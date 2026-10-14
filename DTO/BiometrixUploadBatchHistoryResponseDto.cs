namespace HRIStwinbeeAPI.DTO
{
    public class BiometrixUploadBatchHistoryResponseDto
    {
        public long UploadBatchId { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string DateFrom { get; set; } = string.Empty;
        public string DateTo { get; set; } = string.Empty;
        public int RawLineCount { get; set; }
        public int ParserValidLineCount { get; set; }
        public int ParserInvalidLineCount { get; set; }
        public int StagedPunchCount { get; set; }
        public int ProcessedRowCount { get; set; }
        public string ProcessingStatus { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public string? UploadedBy { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
        public string? ProcessedAt { get; set; }
    }
}
