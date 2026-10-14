using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using HRIStwinbeeAPI.DTO.Inventory.ItemWarehouse;
using HRIStwinbeeAPI.DTO.MaterialIssuance;
using HRIStwinbeeAPI.Repo.Interface.MaterialIssuance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
//using System.Data.SqlClient;
using Microsoft.Data.SqlClient;

namespace HRIStwinbeeAPI.Controllers.MaterialIssuance;

[ApiController]
//[Authorize(Policy = "MaterialIssuance.View")]
[Route("api/inventory/material-issuances")]
public sealed class MaterialIssuanceController(IMaterialIssuanceRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<
    ActionResult<
        ApiResponseDto<
            MaterialIssuancePagedResult<MaterialIssuanceListItemDto>>>>
    Query(
        [FromQuery] MaterialIssuanceQueryRequest request,
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

        try
        {
            var result = await repository.QueryAsync(
                request,
                token);

            return Ok(
                Wrap(
                    result,
                    "Material issuances loaded."));
        }
        catch (SqlException exception)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Material issuance SQL query failed.",
                    Detail = exception.Message
                });
        }
        catch (Exception exception)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Material issuance query failed.",
                    Detail = exception.ToString()
                });
        }
    }

    [HttpGet("{issueNo}")]
    public async Task<ActionResult<ApiResponseDto<MaterialIssuanceDocumentDto>>> Get(string issueNo, CancellationToken token)
    {
        var value = await repository.GetByNoAsync(issueNo, token);
        return value is null ? NotFound(Wrap<MaterialIssuanceDocumentDto>(null, $"Material issuance '{issueNo}' was not found.", false))
            : Ok(Wrap(value, "Material issuance loaded."));
    }

    [HttpGet("lookups/warehouses")]
    public async Task<IActionResult> Warehouses([FromQuery] string? search, CancellationToken token) =>
        Ok(Wrap(await repository.GetWarehousesAsync(search, token), "Warehouses loaded."));
    [HttpGet("lookups/suppliers")]
    public async Task<IActionResult> Suppliers([FromQuery] string? search, CancellationToken token) =>
        Ok(Wrap(await repository.GetSuppliersAsync(search, token), "Suppliers loaded."));

    [HttpGet("lookups/so-verify")]
    public ActionResult<ApiResponseDto<IReadOnlyList<MaterialIssuanceSoVerificationDto>>>
    VerifySoEmpty()
    {
        IReadOnlyList<MaterialIssuanceSoVerificationDto> rows =
            Array.Empty<MaterialIssuanceSoVerificationDto>();

        return Ok(
            Wrap(
                rows,
                "No SO number supplied."));
    }


    [HttpGet("lookups/so-verify/{soNo}")]
    public async Task<IActionResult> VerifySoNo(
        string soNo,
        CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(soNo))
        {
            return Ok(
                Wrap(
                    Array.Empty<MaterialIssuanceSoVerificationDto>(),
                    "No SO number supplied."));
        }

        var result =
            await repository.VerifySoNoAsync(
                soNo.Trim(),
                token);

        return Ok(
            Wrap(
                result,
                result.Count > 0
                    ? "Sales Order verified."
                    : "Sales Order was not found or is no longer open."));
    }

    [HttpGet("lookups/work-centers")]
    public async Task<IActionResult> WorkCenters(
    [FromQuery] string? search,
    CancellationToken token) =>
    Ok(Wrap(
        await repository.GetWorkCentersAsync(search, token),
        "Work centers loaded."));
    [HttpGet("lookups/available-materials")]
    public async Task<IActionResult> Materials([FromQuery][Required] string whseCode, [FromQuery] string? search, CancellationToken token) =>
        Ok(Wrap(await repository.GetAvailableMaterialsAsync(whseCode, search, token), "Available materials loaded."));
    [HttpGet("lookups/fifo-preview")]
    public async Task<IActionResult> Fifo([FromQuery][Required] string whseCode, [FromQuery][Required] string itemCode,
        [FromQuery] decimal? baseQty, CancellationToken token) =>
        Ok(Wrap(await repository.GetFifoPreviewAsync(whseCode, itemCode, baseQty, token), "FIFO preview loaded."));

    [HttpPost("upsert")]
    [Authorize(Policy = "MaterialIssuance.Edit")]
    public async Task<IActionResult> Upsert([FromBody] UpsertMaterialIssuanceRequest request, CancellationToken token)
    {
        if (request.SourceType == "DIR" && !HasPermission("INVENTORY_ISSUE_DIRECT")) return Forbid();
        if (
     request.TargetStatus == "A" &&
     !HasPermission("MAT_ISSUE_ENTRY") &&
     !HasPermission("INVENTORY_ISSUE_POST")
 )
        {
            return Forbid();
        }
        request.UserId = ResolveUserId(request.UserId);
        if (string.IsNullOrWhiteSpace(request.UserId))
            return Validation("userId", "The authenticated user could not be identified.");
        try {
            var result = await repository.UpsertAsync(request, token);
            return Ok(Wrap(result, result.Message));
        }
        catch (ArgumentException e)
        {
            return BadRequest(new ProblemDetails { Status = 400,
            Title = "Invalid material issuance request.", Detail = e.Message });
        }
        catch (SqlException e)
        when (e.Number is >= 51100 and <= 51109)
        {
            return BadRequest(new ProblemDetails { Status = 400, Title = "Material issuance validation failed.", Detail = e.Message });
        }
        catch (SqlException e) when (e.Number >= 51110)
        {
            return Conflict(new ProblemDetails { Status = 409, Title = "Material issuance was rejected.", Detail = e.Message });
        }
    }

    [HttpPost("{issueNo}/reverse")]
    [Authorize(Policy = "MaterialIssuance.Reverse")]
    public async Task<IActionResult> Reverse(string issueNo, [FromBody] ReverseMaterialIssuanceRequest request, CancellationToken token)
    {
        request.UserId = ResolveUserId(request.UserId);
        if (string.IsNullOrWhiteSpace(request.UserId)) return Validation("userId", "The authenticated user could not be identified.");
        try { var result = await repository.ReverseAsync(issueNo, request, token); return Ok(Wrap(result, result.Message)); }
        catch (ArgumentException e) { return BadRequest(new ProblemDetails { Status = 400, Title = "Invalid reversal request.", Detail = e.Message }); }
        catch (SqlException e) when (e.Number >= 51100) { return Conflict(new ProblemDetails { Status = 409, Title = "Material issuance reversal was rejected.", Detail = e.Message }); }
    }

    private bool HasPermission(string key) =>
        string.Equals(User.FindFirstValue("administrator"), "Y", StringComparison.OrdinalIgnoreCase) ||
        User.Claims.Any(c => c.Type == "erp_permission" && string.Equals(c.Value, key, StringComparison.OrdinalIgnoreCase));
    private string? ResolveUserId(string? fallback)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id") ?? User.FindFirstValue("sub") ?? User.Identity?.Name;
        return !string.IsNullOrWhiteSpace(claim) ? claim.Trim() : (string.IsNullOrWhiteSpace(fallback) ? null : fallback.Trim());
    }
    private ActionResult Validation(string key, string message) { ModelState.AddModelError(key, message); return ValidationProblem(ModelState); }
    private static ApiResponseDto<T> Wrap<T>(T? data, string message, bool success = true) => new() { Success = success, Message = message, Data = data };
}
