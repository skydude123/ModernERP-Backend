namespace HRIStwinbeeAPI.DTO
{
    public class BiometrixAttLogUploadRangeRequestDto
    {
        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }

        // MODIFICATION:
        // From Angular FormData: payrollPeriod
        public int? PayrollPeriod { get; set; }

        // MODIFICATION:
        // From Angular FormData: branchName
        public string? BranchName { get; set; }

        public string? UploadedBy { get; set; }

        public IFormFile? File { get; set; }
    }
}
