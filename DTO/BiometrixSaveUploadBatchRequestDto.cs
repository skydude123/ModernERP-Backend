using System.Data;

namespace HRIStwinbeeAPI.DTO
{
    public class BiometrixSaveUploadBatchRequestDto
    {
        public string OriginalFileName { get; set; } = string.Empty;

        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }

        public long FileSizeBytes { get; set; }

        public int RawLineCount { get; set; }

        public int ParserValidLineCount { get; set; }

        public int ParserInvalidLineCount { get; set; }

        public string? UploadedBy { get; set; }

        // MODIFICATION:
        // Save selected payroll period to upload batch.
        public int? PayrollPeriod { get; set; }

        // MODIFICATION:
        // Save selected branch to upload batch.
        public string? BranchName { get; set; }

        public DataTable RawPunchTable { get; set; } = new();
    }
}
