using System.Security.Claims;
using HRIStwinbeeAPI.DTO.Inventory.ItemWarehouse;
using HRIStwinbeeAPI.DTO.Receiving;
using HRIStwinbeeAPI.Repo.Interface.Receiving;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace HRIStwinbeeAPI.Controllers.Receiving;

[ApiController]
[Route("api/accounting/receivings")]
[Route("api/receiving")]
public sealed class ReceivingController(
    IReceivingRepository repository,
    IWebHostEnvironment environment) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResult<ReceivingListItemDto>>>> Query(
    [FromQuery] ReceivingQueryRequest request,
    CancellationToken cancellationToken)
    {
        if (request.DateFrom?.Date > request.DateTo?.Date)
        {
            ModelState.AddModelError(
                nameof(request.DateFrom),
                "DateFrom cannot be later than DateTo.");

            return ValidationProblem(ModelState);
        }

        var userId = ResolveUserId(request.UserId);

        if (string.IsNullOrWhiteSpace(userId))
        {
            ModelState.AddModelError(
                nameof(request.UserId),
                "The authenticated user could not be identified.");

            return ValidationProblem(ModelState);
        }

        request.UserId = userId;

        var result = await repository.QueryAsync(
            request,
            cancellationToken);

        return Ok(new ApiResponseDto<PagedResult<ReceivingListItemDto>>
        {
            Success = true,
            Message = "Receiving records loaded successfully.",
            Data = result
        });
    }

    [HttpGet("{rrNo}")]
    public async Task<ActionResult<ApiResponseDto<ReceivingDocumentDto?>>> GetByNo(
        string rrNo,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetByNoAsync(rrNo, cancellationToken);

        if (result is null)
        {
            return NotFound(new ApiResponseDto<ReceivingDocumentDto?>
            {
                Success = false,
                Message = $"Receiving number '{rrNo.Trim()}' was not found.",
                Data = null
            });
        }

        return Ok(new ApiResponseDto<ReceivingDocumentDto?>
        {
            Success = true,
            Message = "Receiving document loaded successfully.",
            Data = result
        });
    }

    [HttpGet("lookups/suppliers")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<SupplierLookupDto>>>> GetSuppliers(
        CancellationToken cancellationToken) =>
        Ok(new ApiResponseDto<IReadOnlyList<SupplierLookupDto>>
        {
            Success = true,
            Message = "Supplier lookup loaded successfully.",
            Data = await repository.GetSuppliersAsync(cancellationToken)
        });

    [HttpGet("lookups/branches")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<BranchLookupDto>>>> GetBranches(
        CancellationToken cancellationToken) =>
        Ok(new ApiResponseDto<IReadOnlyList<BranchLookupDto>>
        {
            Success = true,
            Message = "Branch lookup loaded successfully.",
            Data = await repository.GetBranchesAsync(cancellationToken)
        });

    [HttpGet("lookups/warehouses")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<WarehouseLookupDto>>>> GetWarehouses(
        CancellationToken cancellationToken) =>
        Ok(new ApiResponseDto<IReadOnlyList<WarehouseLookupDto>>
        {
            Success = true,
            Message = "Warehouse lookup loaded successfully.",
            Data = await repository.GetWarehousesAsync(cancellationToken)
        });

    [HttpGet("lookups/open-po-lines")]
    public async Task<ActionResult<ApiResponseDto<PagedResult<OpenPoLineDto>>>> GetOpenPoLines(
        [FromQuery] OpenPoQueryRequest request,
        CancellationToken cancellationToken) =>
        Ok(new ApiResponseDto<PagedResult<OpenPoLineDto>>
        {
            Success = true,
            Message = "Open purchase order lines loaded successfully.",
            Data = await repository.GetOpenPoLinesAsync(request, cancellationToken)
        });

    //[HttpGet("lookups/so-verify/{soNo}")]
    //public async Task<ActionResult<ApiResponseDto<IReadOnlyList<SoVerificationDto>>>> VerifySoNo(
    //string soNo,
    //CancellationToken cancellationToken)
    //{
    //    if (string.IsNullOrWhiteSpace(soNo))
    //    {
    //        return Ok(
    //            new ApiResponseDto<IReadOnlyList<SoVerificationDto>>
    //            {
    //                Success = true,
    //                Message = "SO number is empty.",
    //                Data = Array.Empty<SoVerificationDto>()
    //            });
    //    }

    //    var result =
    //        await repository.VerifySoNoAsync(
    //            soNo.Trim(),
    //            cancellationToken);

    //    return Ok(
    //        new ApiResponseDto<IReadOnlyList<SoVerificationDto>>
    //        {
    //            Success = true,
    //            Message =
    //                result.Count > 0
    //                    ? "SO number verified successfully."
    //                    : "No valid open Sales Order was found.",
    //            Data = result
    //        });
    //}

    [HttpGet("report-status")]
    public IActionResult GetReportStatus()
    {
        var reportPath = Path.Combine(
            environment.ContentRootPath,
            "Reports",
            "ReceivingReport.trdp");

        if (!System.IO.File.Exists(reportPath))
        {
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Receiving report is not installed.",
                detail:
                    "Create Reports/ReceivingReport.trdp with the licensed Telerik Report Designer " +
                    "and the TwinBeeErpReporting named connection.");
        }

        return Ok(new { available = true });
    }

    [HttpGet("lookups/items")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<ReceivingItemLookupDto>>>> GetItems(
    [FromQuery] string? search,
    [FromQuery] string? whseCode,
    CancellationToken cancellationToken)
    {
        var result = await repository.GetItemsAsync(
            search,
            whseCode,
            cancellationToken);

        return Ok(
            new ApiResponseDto<IReadOnlyList<ReceivingItemLookupDto>>
            {
                Success = true,
                Message = "Receiving item lookup loaded successfully.",
                Data = result
            });
    }

    [HttpPost("upsert")]
    public async Task<ActionResult<ApiResponseDto<ReceivingSaveResultDto>>> Upsert(
     [FromBody] UpsertReceivingRequest request,
     CancellationToken cancellationToken)
    {
        if (request.TransType == "RPO" &&
            string.IsNullOrWhiteSpace(request.SuppCode))
        {
            ModelState.AddModelError(
                nameof(request.SuppCode),
                "Supplier is required for RPO.");

            return ValidationProblem(ModelState);
        }

        var userId = ResolveUserId(request.UserId);

        if (string.IsNullOrWhiteSpace(userId))
        {
            ModelState.AddModelError(
                nameof(request.UserId),
                "The authenticated user could not be identified.");

            return ValidationProblem(ModelState);
        }

        request.UserId = userId;

        try
        {
            var result = await repository.UpsertBatchAsync(
                request,
                cancellationToken);

            return Ok(new ApiResponseDto<ReceivingSaveResultDto>
            {
                Success = true,
                Message = result.Message,
                Data = result
            });
        }
        // Stored procedure THROW/RAISERROR business validation
        catch (SqlException exception) when (exception.Number >= 50000)
        {
            return ConflictProblem(
                "Receiving transaction was rejected.",
                exception.Message);
        }
        // Other SQL/database errors
        catch (SqlException exception)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponseDto<ReceivingSaveResultDto>
                {
                    Success = false,
                    Message =
                        $"A database error occurred while saving the receiving transaction. " +
                        $"SQL Error {exception.Number}: {exception.Message}",
                    Data = null
                });
        }
        // Request was cancelled by the client/server
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            return StatusCode(
                StatusCodes.Status499ClientClosedRequest,
                new ApiResponseDto<ReceivingSaveResultDto>
                {
                    Success = false,
                    Message = "The receiving save request was cancelled.",
                    Data = null
                });
        }
        // Any other unexpected error
        catch (Exception exception)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponseDto<ReceivingSaveResultDto>
                {
                    Success = false,
                    Message =
                        $"An unexpected error occurred while saving the receiving transaction: " +
                        exception.Message,
                    Data = null
                });
        }
    }

    [HttpPost("{rrNo}/reverse")]
    public async Task<ActionResult<ApiResponseDto<ReceivingSaveResultDto>>> Reverse(
        string rrNo,
        [FromBody] ReverseReceivingRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId(request.UserId);
        if (string.IsNullOrWhiteSpace(userId))
        {
            ModelState.AddModelError(
                nameof(request.UserId),
                "The authenticated user could not be identified.");
            return ValidationProblem(ModelState);
        }

        request.UserId = userId;

        try
        {
            var result = await repository.ReverseAsync(
                rrNo,
                request,
                cancellationToken);

            return Ok(new ApiResponseDto<ReceivingSaveResultDto>
            {
                Success = true,
                Message = result.Message,
                Data = result
            });
        }
        catch (SqlException exception) when (exception.Number >= 50000)
        {
            return ConflictProblem(
                "Receiving reversal was rejected.",
                exception.Message);
        }
    }

    private string? ResolveUserId(string? fallback)
    {
        var claimValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("user_id") ??
            User.FindFirstValue("sub") ??
            User.Identity?.Name;

        if (!string.IsNullOrWhiteSpace(claimValue))
        {
            return claimValue.Trim();
        }

        // JWT authentication is currently disabled in Program.cs. Remove this fallback
        // after authentication is restored so command user IDs come only from claims.
        return string.IsNullOrWhiteSpace(fallback) ? null : fallback.Trim();
    }

    private ObjectResult ConflictProblem(string title, string detail)
    {
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = title,
            Detail = detail,
            Type = "https://httpstatuses.com/409",
            Instance = HttpContext.Request.Path
        };

        return StatusCode(StatusCodes.Status409Conflict, problem);
    }
}
