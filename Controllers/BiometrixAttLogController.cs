using HRIStwinbeeAPI.DTO;
using HRIStwinbeeAPI.Helpers;
using HRIStwinbeeAPI.Parsers;
using HRIStwinbeeAPI.Repo.Interface;
using Microsoft.AspNetCore.Mvc;

namespace HRIStwinbeeAPI.Controllers
{
    [Route("api/biometrix/attlog")]
    [ApiController]
    public class BiometrixAttLogController : ControllerBase
    {
        private readonly BiometrixAttLogParser _parser;
        private readonly IBiometrixAttLogRepository _repository;

        public BiometrixAttLogController(
            BiometrixAttLogParser parser,
            IBiometrixAttLogRepository repository)
        {
            _parser = parser;
            _repository = repository;
        }

        [HttpGet("payroll-periods")]
        [ProducesResponseType(typeof(IReadOnlyList<BiometrixPayrollPeriodDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<BiometrixPayrollPeriodDto>>> GetPayrollPeriods(
            CancellationToken cancellationToken)
        {
            var rows = await _repository.GetPayrollPeriodsAsync(cancellationToken);
            return Ok(rows);
        }

        [HttpGet("branches")]
        [ProducesResponseType(typeof(IReadOnlyList<BiometrixBranchDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<BiometrixBranchDto>>> GetBranches(
            CancellationToken cancellationToken)
        {
            var rows = await _repository.GetBranchesAsync(cancellationToken);
            return Ok(rows);
        }

        [HttpGet("batch/{uploadBatchId:long}")]
        [ProducesResponseType(typeof(BiometrixUploadBatchDetailResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BiometrixUploadBatchDetailResponseDto>> GetBatch(
            long uploadBatchId,
            CancellationToken cancellationToken)
        {
            var response = await _repository.GetBatchByIdAsync(uploadBatchId, cancellationToken);

            if (response == null)
            {
                return NotFound(new { message = $"Upload batch {uploadBatchId} was not found." });
            }

            return Ok(response);
        }

        [HttpGet("history")]
        [ProducesResponseType(typeof(IReadOnlyList<BiometrixUploadBatchHistoryResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<BiometrixUploadBatchHistoryResponseDto>>> GetHistory(
            [FromQuery] int take = 25,
            CancellationToken cancellationToken = default)
        {
            if (take <= 0)
            {
                take = 25;
            }

            if (take > 200)
            {
                take = 200;
            }

            var rows = await _repository.GetHistoryAsync(take, cancellationToken);
            return Ok(rows);
        }

        [HttpPost("upload-range")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(BiometrixUploadBatchDetailResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BiometrixUploadBatchDetailResponseDto>> UploadRange(
            [FromForm] BiometrixAttLogUploadRangeRequestDto request,
            CancellationToken cancellationToken)
        {
            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest(new { message = "ATTLOG.DAT file is required." });
            }

            if (request.DateFrom == default || request.DateTo == default)
            {
                return BadRequest(new { message = "Date From and Date To are required." });
            }

            if (request.DateFrom.Date > request.DateTo.Date)
            {
                return BadRequest(new { message = "Date From cannot be later than Date To." });
            }

            if (request.PayrollPeriod == null || request.PayrollPeriod <= 0)
            {
                return BadRequest(new { message = "Payroll Period is required." });
            }

            if (string.IsNullOrWhiteSpace(request.BranchName))
            {
                return BadRequest(new { message = "Branch Name is required." });
            }

            var extension = Path.GetExtension(request.File.FileName);
            if (!string.Equals(extension, ".dat", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Only .DAT files are allowed." });
            }

            try
            {
                await using var fileStream = request.File.OpenReadStream();

                var parserResult = await _parser.ParseAsync(
                    fileStream,
                    request.DateFrom,
                    request.DateTo,
                    cancellationToken);

                if (parserResult.Punches.Count == 0)
                {
                    return BadRequest(new
                    {
                        message = "The file contains no valid or auditable rows for the selected date range."
                    });
                }

                var saveRequest = new BiometrixSaveUploadBatchRequestDto
                {
                    OriginalFileName = request.File.FileName,
                    DateFrom = request.DateFrom.Date,
                    DateTo = request.DateTo.Date,
                    FileSizeBytes = request.File.Length,
                    RawLineCount = parserResult.RawLineCount,
                    ParserValidLineCount = parserResult.ParserValidLineCount,
                    ParserInvalidLineCount = parserResult.ParserInvalidLineCount,
                    UploadedBy = request.UploadedBy,

                    // MODIFICATION:
                    // Save selected payroll period and branch from Angular.
                    PayrollPeriod = request.PayrollPeriod,
                    BranchName = request.BranchName.Trim(),

                    RawPunchTable = BiometrixRawPunchTvpDataTableFactory.Create(parserResult.Punches)
                };

                var uploadBatchId = await _repository.SaveUploadBatchAsync(saveRequest, cancellationToken);

                await _repository.ProcessUploadBatchAsync(uploadBatchId, cancellationToken);

                var response = await _repository.GetBatchByIdAsync(uploadBatchId, cancellationToken);

                if (response == null)
                {
                    return Problem(
                        title: "Attendance processing failed",
                        detail: "Upload batch was saved but the processed result could not be loaded.",
                        statusCode: StatusCodes.Status500InternalServerError);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Problem(
                    title: "Attendance upload failed",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut("attendance/ot")]
        [ProducesResponseType(typeof(BiometrixAttendanceOtUpdateResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BiometrixAttendanceOtUpdateResponseDto>> UpdateAttendanceOt(
            [FromBody] BiometrixAttendanceOtUpdateRequestDto request,
            CancellationToken cancellationToken)
        {
            if (request.ProcessedAttendanceId <= 0)
            {
                return BadRequest(new { message = "ProcessedAttendanceId is required." });
            }

            if (request.OtHours < 0)
            {
                return BadRequest(new { message = "OT cannot be negative." });
            }

            if (request.OtHours > 24)
            {
                return BadRequest(new { message = "OT cannot exceed 24 hours." });
            }

            request.UpdatedBy = string.IsNullOrWhiteSpace(request.UpdatedBy)
                ? "SYSTEM"
                : request.UpdatedBy.Trim();

            try
            {
                var response = await _repository.UpdateAttendanceOtAsync(request, cancellationToken);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Problem(
                    title: "OT update failed",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}