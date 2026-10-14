using HRIStwinbeeAPI.DTO;
using HRIStwinbeeAPI.Repo.Interface;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HRIStwinbeeAPI.Controllers
{
    [Route("api/biometrix/dtr-listing")]
    [ApiController]
    public class BiometrixDtrListingController : ControllerBase
    {

        private readonly IBiometrixDtrListingRepository _repository;

        public BiometrixDtrListingController(IBiometrixDtrListingRepository repository)
        {
            _repository = repository;
        }


        [HttpGet("branches")]
        [ProducesResponseType(typeof(IReadOnlyList<BiometrixLookupItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<BiometrixLookupItemDto>>> GetBranches(CancellationToken cancellationToken)
        {
            var rows = await _repository.GetBranchesAsync(cancellationToken);
            return Ok(rows);
        }

        [HttpGet("payroll-periods")]
        [ProducesResponseType(typeof(IReadOnlyList<BiometrixLookupItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<BiometrixLookupItemDto>>> GetPayrollPeriods(CancellationToken cancellationToken)
        {
            var rows = await _repository.GetPayrollPeriodsAsync(cancellationToken);
            return Ok(rows);
        }

        [HttpGet("list")]
        [ProducesResponseType(typeof(IReadOnlyList<BiometrixDtrListingRowDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IReadOnlyList<BiometrixDtrListingRowDto>>> GetList(
            [FromQuery] string branchName = "ALL",
            [FromQuery] int? payrollPeriod = null,
            CancellationToken cancellationToken = default)
        {
            if (payrollPeriod is null)
            {
                return BadRequest(new { message = "Payroll period is required." });
            }

            var rows = await _repository.GetListingAsync(new BiometrixDtrListingFilterDto
            {
                BranchName = string.IsNullOrWhiteSpace(branchName) ? "ALL" : branchName,
                PayrollPeriod = payrollPeriod.Value
            }, cancellationToken);

            return Ok(rows);
        }

        [HttpPut("{dtrId:long}")]
        [ProducesResponseType(typeof(BiometrixDtrListingRowDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BiometrixDtrListingRowDto>> Update(
            long dtrId,
            [FromBody] BiometrixDtrListingUpdateDto request,
            CancellationToken cancellationToken = default)
        {
            if (dtrId <= 0)
            {
                return BadRequest(new { message = "Invalid DTR ID." });
            }

            if (request == null)
            {
                return BadRequest(new { message = "Update payload is required." });
            }

            request.DtrId = dtrId;

            var row = await _repository.UpdateAsync(request, cancellationToken);
            if (row == null)
            {
                return NotFound(new { message = "DTR record was not found." });
            }

            return Ok(row);
        }
    }
}
