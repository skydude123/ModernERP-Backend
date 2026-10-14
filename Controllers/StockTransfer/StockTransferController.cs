using System.Security.Claims;
using HRIStwinbeeAPI.DTO.Inventory.ItemWarehouse;
using HRIStwinbeeAPI.DTO.StockTransfer;
using HRIStwinbeeAPI.Repo.Interface.StockTransfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace HRIStwinbeeAPI.Controllers.StockTransfer;

[ApiController]
//[Authorize(Policy = "StockTransfer.View")]
[Route("api/inventory/stock-transfers")]
public sealed class StockTransferController(IStockTransferRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<StockTransferPagedResult<StockTransferListItemDto>>>> Query(
     [FromQuery] StockTransferQueryRequest request,
     CancellationToken token)
    {
        if (request.DateFrom?.Date > request.DateTo?.Date)
        {
            return Validation(
                "dateFrom",
                "DateFrom cannot be later than DateTo.");
        }

        request.UserId = ResolveUserId(request.UserId);

        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            return Validation(
                "userId",
                "The authenticated user could not be identified.");
        }

        var result = await repository.QueryAsync(
            request,
            token);

        return Ok(
            Wrap(
                result,
                "Stock transfers loaded."));
    }

    [HttpGet("{stNo}")]
    public async Task<ActionResult<ApiResponseDto<StockTransferDocumentDto>>> Get(
        string stNo,
        CancellationToken token)
    {
        var value = await repository.GetByNoAsync(stNo, token);
        return value is null
            ? NotFound(Wrap<StockTransferDocumentDto>(null, $"Stock transfer '{stNo}' was not found.", false))
            : Ok(Wrap(value, "Stock transfer loaded."));
    }

    [HttpGet("lookups/warehouses")]
    public async Task<IActionResult> Warehouses([FromQuery] string? search, CancellationToken token) =>
        Ok(Wrap(await repository.GetWarehousesAsync(search, token), "Storage locations loaded from dbo.a_whse."));

    [HttpGet("lookups/source-items")]
    public async Task<IActionResult> SourceItems(
        [FromQuery] string sourceWhseCode,
        [FromQuery] string? search,
        CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(sourceWhseCode))
        {
            return Validation("sourceWhseCode", "Source warehouse is required.");
        }

        return Ok(Wrap(
            await repository.GetSourceItemsAsync(sourceWhseCode, search, token),
            "Source warehouse items loaded."));
    }

    [HttpPost("upsert")]
    //[Authorize(Policy = "StockTransfer.Edit")]
    public async Task<IActionResult> Upsert(
        [FromBody] UpsertStockTransferRequest request,
        CancellationToken token)
    {
        if (request.TargetStatus == "U" && !HasPermission("STOCK_TRANSFER_NEW"))
        {
            return Forbid();
        }

        request.UserId = ResolveUserId(request.UserId);
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            return Validation("userId", "The authenticated user could not be identified.");
        }

        try
        {
            var result = await repository.UpsertAsync(request, token);
            return Ok(Wrap(result, result.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid stock-transfer request.",
                Detail = exception.Message
            });
        }
        catch (SqlException exception) when (exception.Number is >= 51200 and <= 51214)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Stock-transfer validation failed.",
                Detail = exception.Message
            });
        }
        catch (SqlException exception) when (exception.Number >= 51215)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Stock transfer was rejected.",
                Detail = exception.Message
            });
        }
    }

    [HttpPost("{stNo}/reverse")]
    //[Authorize(Policy = "StockTransfer.Edit")]
    public async Task<IActionResult> Reverse(
    string stNo,
    [FromBody] ReverseStockTransferRequest request,
    CancellationToken token)
    {
        if (!HasPermission("STOCK_TRANSFER_NEW"))
        {
            return Forbid();
        }

        var routeStNo =
            String.IsNullOrWhiteSpace(stNo)
                ? null
                : stNo.Trim();

        if (routeStNo is null)
        {
            return Validation(
                "stNo",
                "Stock Transfer number is required.");
        }

        request.StNo = routeStNo;

        request.UserId =
            ResolveUserId(request.UserId);

        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            return Validation(
                "userId",
                "The authenticated user could not be identified.");
        }

        if (string.IsNullOrWhiteSpace(request.Reason) ||
            request.Reason.Trim().Length < 5)
        {
            return Validation(
                "reason",
                "Reverse reason must contain at least 5 characters.");
        }

        try
        {
            var result =
                await repository.ReverseAsync(
                    request,
                    token);

            return Ok(
                Wrap(
                    result,
                    result.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(
                new ProblemDetails
                {
                    Status =
                        StatusCodes.Status400BadRequest,

                    Title =
                        "Invalid Stock Transfer reverse request.",

                    Detail =
                        exception.Message
                });
        }
        catch (SqlException exception)
            when (
                exception.Number >= 51300 &&
                exception.Number <= 51308
            )
        {
            return BadRequest(
                new ProblemDetails
                {
                    Status =
                        StatusCodes.Status400BadRequest,

                    Title =
                        "Stock Transfer reversal validation failed.",

                    Detail =
                        exception.Message
                });
        }
        catch (SqlException exception)
            when (
                exception.Number >= 51309 &&
                exception.Number <= 51399
            )
        {
            return Conflict(
                new ProblemDetails
                {
                    Status =
                        StatusCodes.Status409Conflict,

                    Title =
                        "Stock Transfer cannot be reversed.",

                    Detail =
                        exception.Message
                });
        }
    }

    private bool HasPermission(string key) =>
        string.Equals(User.FindFirstValue("administrator"), "Y", StringComparison.OrdinalIgnoreCase)
        || User.Claims.Any(claim =>
            claim.Type == "erp_permission"
            && string.Equals(claim.Value, key, StringComparison.OrdinalIgnoreCase));

    private string? ResolveUserId(string? fallback)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("user_id")
                    ?? User.FindFirstValue("sub")
                    ?? User.Identity?.Name;

        return !string.IsNullOrWhiteSpace(claim)
            ? claim.Trim()
            : string.IsNullOrWhiteSpace(fallback) ? null : fallback.Trim();
    }

    private ActionResult Validation(string key, string message)
    {
        ModelState.AddModelError(key, message);
        return ValidationProblem(ModelState);
    }

    private static ApiResponseDto<T> Wrap<T>(T? data, string message, bool success = true) =>
        new() { Success = success, Message = message, Data = data };
}
