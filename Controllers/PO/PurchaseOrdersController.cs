using HRIStwinbeeAPI.DTO.PO;
using HRIStwinbeeAPI.Repo.Interface.PO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HRIStwinbeeAPI.Controllers.PO
{
    [Route("api/purchase-orders")]
    [ApiController]
    public class PurchaseOrdersController : ControllerBase
    {
        private readonly IPurchaseOrderRepository _repository;

        public PurchaseOrdersController(IPurchaseOrderRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("lookups/suppliers")]
        public async Task<ActionResult<IReadOnlyList<SupplierLookupDto>>> GetSuppliers()
        {
            var result = await _repository.GetSuppliersAsync();
            return Ok(result);
        }

        [HttpGet("lookups/branches")]
        public async Task<ActionResult<IReadOnlyList<BranchLookupDto>>> GetBranches()
        {
            var result = await _repository.GetBranchesAsync();
            return Ok(result);
        }

        [HttpGet("lookups/terms")]
        public async Task<ActionResult<IReadOnlyList<TermLookupDto>>> GetTerms()
        {
            var result = await _repository.GetTermsAsync();
            return Ok(result);
        }

        [HttpGet("lookups/vat-types")]
        public async Task<ActionResult<IReadOnlyList<VatTypeLookupDto>>> GetVatTypes()
        {
            var result = await _repository.GetVatTypesAsync();
            return Ok(result);
        }

        [HttpGet("lookups/items")]
        public async Task<ActionResult<IReadOnlyList<ItemLookupDto>>> GetItems([FromQuery] string? search)
        {
            var result = await _repository.GetItemsAsync(search);
            return Ok(result);
        }

        [HttpGet("lookups/um")]
        public async Task<ActionResult<IReadOnlyList<UmDto>>> GetUm()
        {
            var result = await _repository.GetUmAsync();
            return Ok(result);
        }

        [HttpGet("lookups/so-verify")]
        public ActionResult<IReadOnlyList<SoVerificationDto>> VerifySoNoEmpty()
        {
            return Ok(Array.Empty<SoVerificationDto>());
        }

        [HttpGet("lookups/so-verify/{soNo}")]
        public async Task<ActionResult<IReadOnlyList<SoVerificationDto>>> VerifySoNo(string soNo)
        {
            if (string.IsNullOrWhiteSpace(soNo))
                return Ok(Array.Empty<SoVerificationDto>());

            var result = await _repository.VerifySoNoAsync(soNo);
            return Ok(result);
        }
        [HttpGet("{poNo}")]
        public async Task<ActionResult<PurchaseOrderGetResponseDto>> GetByNo(string poNo)
        {
            var result = await _repository.GetByNoAsync(poNo);

            if (result.Header is null)
                return NotFound(new { message = $"PO number {poNo} was not found." });

            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<PurchaseOrderListItemDto>>> GetListPage(
     [FromQuery] string? userId = null,
     [FromQuery] DateTime? fromDate = null,
     [FromQuery] DateTime? toDate = null,
     [FromQuery] string status = "ALL",
     [FromQuery] string? branchName = null,
     [FromQuery] string? search = null,
     [FromQuery] int skip = 0,
     [FromQuery] int take = 50,
     [FromQuery] string sortField = "poDate",
     [FromQuery] string sortDir = "desc")
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "The logged-in user ID is required."
                });
            }

            var query = new PurchaseOrderListQueryDto
            {
                FromDate = fromDate,
                ToDate = toDate,
                Status = status,
                BranchName = branchName,
                Search = search,
                Skip = skip,
                Take = take,
                SortField = sortField,
                SortDir = sortDir,
                UserId = userId.Trim().ToUpperInvariant()
            };

            var result = await _repository.GetListPageAsync(query);

            return Ok(result);
        }

        //[HttpPost("save")]
        //public async Task<ActionResult<PurchaseOrderSaveResponseDto>> Save([FromBody] PurchaseOrderSaveRequestDto request)
        //{
        //    if (request is null)
        //        return BadRequest(new { message = "Request body is required." });

        //    if (request.Details.Count == 0)
        //        return BadRequest(new { message = "PO must have at least one detail line." });

        //    var result = await _repository.SaveAsync(request);
        //    return Ok(result);
        //}

        [HttpPost("save")]
        public async Task<ActionResult<PurchaseOrderSaveResponseDto>> Save(
    [FromBody] PurchaseOrderSaveRequestDto request,
    CancellationToken cancellationToken)
        {
            if (request is null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Request body is required."
                });
            }

            if (request.Details is null ||
                request.Details.Count == 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "PO must have at least one detail line."
                });
            }

            try
            {
                var result =
                    await _repository.SaveAsync(request, cancellationToken);

                return Ok(result);
            }
            /*
             * Invalid request values detected by the
             * repository or business layer.
             */
            catch (ArgumentException exception)
            {
                return BadRequest(new
                {
                    success = false,
                    message = exception.Message
                });
            }
            /*
             * Business-rule conflict raised by the application.
             *
             * Examples:
             * - PO cannot be edited because it is approved
             * - Duplicate transaction
             * - Invalid current status
             */
            catch (InvalidOperationException exception)
            {
                return Conflict(new
                {
                    success = false,
                    message = exception.Message
                });
            }
            /*
             * Custom SQL Server errors raised from a stored
             * procedure through THROW or RAISERROR.
             */
            catch (Microsoft.Data.SqlClient.SqlException exception)
                when (exception.Number >= 50000)
            {
                return Conflict(new
                {
                    success = false,
                    message = exception.Message
                });
            }
            /*
             * Other SQL Server/database errors.
             */
            catch (Microsoft.Data.SqlClient.SqlException exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        success = false,
                        message =
                            $"A database error occurred while saving the Purchase Order. " +
                            $"SQL Error {exception.Number}: {exception.Message}"
                    });
            }
            /*
             * Fallback for any other unexpected error.
             */
            catch (Exception exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        success = false,
                        message =
                            $"An unexpected error occurred while saving the Purchase Order: " +
                            exception.Message
                    });
            }
        }

        [HttpPost("{poNo}/post")]
        public async Task<ActionResult<PurchaseOrderStatusActionResultDto>> PostPo(
    string poNo,
    [FromBody] PurchaseOrderStatusActionRequestDto request)
        {
            request.PoNo = poNo;
            request.Action = "POST";

            var result = await _repository.StatusActionAsync(request);

            return Ok(result);
        }

        [HttpPost("{poNo}/approve")]
        public async Task<ActionResult<PurchaseOrderStatusActionResultDto>> ApprovePo(
    string poNo,
    [FromBody] PurchaseOrderStatusActionRequestDto request)
        {
            request.PoNo = poNo;
            request.Action = "APPROVE";

            var result = await _repository.StatusActionAsync(request);

            return Ok(result);
        }

        [HttpPost("{poNo}/reopen")]
        public async Task<ActionResult<PurchaseOrderStatusActionResultDto>> ReopenPo(
            string poNo,
            [FromBody] PurchaseOrderStatusActionRequestDto request)
        {
            request.PoNo = poNo;
            request.Action = "REOPEN";

            var result = await _repository.StatusActionAsync(request);

            return Ok(result);
        }

        [HttpPost("reverse-approval")]
        public async Task<IActionResult> ReverseApproval(
    [FromBody] PurchaseOrderReverseApprovalRequestDto request,
    CancellationToken cancellationToken)
        {
            if (request == null)
                return BadRequest(new { message = "Request body is required." });

            if (string.IsNullOrWhiteSpace(request.PoNo))
                return BadRequest(new { message = "PO number is required." });

            if (string.IsNullOrWhiteSpace(request.UserId))
                return BadRequest(new { message = "User ID is required." });

            try
            {
                var result = await _repository.ReverseApprovalAsync(request, cancellationToken);
                return Ok(result);
            }
            catch (SqlException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

    }
}
