using System.Security.Claims;

using HRIStwinbeeAPI.DTO.FuelAcceptance;
using HRIStwinbeeAPI.Repo.Interface.FuelAcceptance;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace HRIStwinbeeAPI.Controllers.FuelAcceptance;


[ApiController]
[Authorize]
[Route("api/fuel-acceptances")]
public sealed class FuelAcceptancesController(
    IFuelAcceptanceRepository repository)
    : ControllerBase
{
    /* ============================================================
       LIST
       ============================================================ */

    [HttpGet]
    public async Task<
        ActionResult<
            FuelApiResponseDto<
                FuelPagedResultDto<FuelAcceptanceListItemDto>>>>
        List(
            [FromQuery] FuelAcceptanceQueryDto query,
            CancellationToken cancellationToken)
    {
        if (
            query.FromDate.HasValue &&
            query.ToDate.HasValue &&
            query.FromDate > query.ToDate)
        {
            return BadRequest(
                FuelApiResponseDto<
                    FuelPagedResultDto<FuelAcceptanceListItemDto>>
                .FromError(
                    "Date From cannot be after Date To."));
        }


        var result =
            await repository.QueryAsync(
                query,
                cancellationToken);


        return Ok(
            FuelApiResponseDto<
                FuelPagedResultDto<FuelAcceptanceListItemDto>>
            .FromData(result));
    }


    /* ============================================================
       GET DOCUMENT
       ============================================================ */

    [HttpGet("{fuelAcceptanceNo}")]
    public async Task<
        ActionResult<
            FuelApiResponseDto<FuelAcceptanceDocumentDto>>>
        GetByNo(
            [FromRoute] string fuelAcceptanceNo,
            CancellationToken cancellationToken)
    {
        if (
            string.IsNullOrWhiteSpace(
                fuelAcceptanceNo))
        {
            return BadRequest(
                FuelApiResponseDto<FuelAcceptanceDocumentDto>
                .FromError(
                    "Fuel Acceptance number is required."));
        }


        var result =
            await repository.GetAsync(
                fuelAcceptanceNo.Trim(),
                cancellationToken);


        return result is null

            ? NotFound(
                FuelApiResponseDto<FuelAcceptanceDocumentDto>
                .FromError(
                    "Fuel Acceptance was not found."))

            : Ok(
                FuelApiResponseDto<FuelAcceptanceDocumentDto>
                .FromData(result));
    }


    /* ============================================================
       SAVE / UPDATE
       ============================================================ */

    [HttpPost]
    public async Task<
        ActionResult<
            FuelApiResponseDto<FuelAcceptanceCommandResultDto>>>
        Save(
            [FromBody] FuelAcceptanceSaveDto request,
            CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await repository.SaveAsync(
                    request,
                    CurrentUserId(),
                    cancellationToken);


            return Ok(
                FuelApiResponseDto<FuelAcceptanceCommandResultDto>
                .FromData(
                    result,
                    result.Message));
        }

        catch (ArgumentException exception)
        {
            return BadRequest(
                FuelApiResponseDto<FuelAcceptanceCommandResultDto>
                .FromError(
                    exception.Message));
        }

        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(
                FuelApiResponseDto<FuelAcceptanceCommandResultDto>
                .FromError(
                    exception.Message));
        }

        catch (SqlException exception)
            when (exception.Number == 51000)
        {
            return BadRequest(
                FuelApiResponseDto<FuelAcceptanceCommandResultDto>
                .FromError(
                    exception.Message));
        }

        catch (SqlException exception)
            when (exception.Number is >= 51001 and <= 51009)
        {
            return Conflict(
                FuelApiResponseDto<FuelAcceptanceCommandResultDto>
                .FromError(
                    exception.Message));
        }
    }


    /* ============================================================
       POST
       ============================================================ */

    [HttpPost("{fuelAcceptanceNo}/post")]
    public async Task<
        ActionResult<
            FuelApiResponseDto<FuelAcceptanceCommandResultDto>>>
        Post(
            [FromRoute] string fuelAcceptanceNo,
            [FromBody] FuelAcceptanceWorkflowDto request,
            CancellationToken cancellationToken)
    {
        if (
            string.IsNullOrWhiteSpace(fuelAcceptanceNo) ||
            string.IsNullOrWhiteSpace(request.RowVersion))
        {
            return BadRequest(
                FuelApiResponseDto<FuelAcceptanceCommandResultDto>
                .FromError(
                    "Fuel Acceptance number and row version are required."));
        }


        try
        {
            var result =
                await repository.PostAsync(
                    fuelAcceptanceNo.Trim(),
                    request.RowVersion.Trim(),
                    CurrentUserId(),
                    cancellationToken);


            return Ok(
                FuelApiResponseDto<FuelAcceptanceCommandResultDto>
                .FromData(
                    result,
                    result.Message));
        }

        catch (SqlException exception)
            when (exception.Number is >= 51000 and <= 51009)
        {
            return Conflict(
                FuelApiResponseDto<FuelAcceptanceCommandResultDto>
                .FromError(
                    exception.Message));
        }

        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(
                FuelApiResponseDto<FuelAcceptanceCommandResultDto>
                .FromError(
                    exception.Message));
        }
    }


    /* ============================================================
       REVERSE
       ============================================================ */

    [HttpPost("{fuelAcceptanceNo}/reverse")]
    public async Task<
        ActionResult<
            FuelApiResponseDto<FuelAcceptanceCommandResultDto>>>
        Reverse(
            [FromRoute] string fuelAcceptanceNo,
            [FromBody] FuelAcceptanceWorkflowDto request,
            CancellationToken cancellationToken)
    {
        if (
            string.IsNullOrWhiteSpace(fuelAcceptanceNo) ||
            string.IsNullOrWhiteSpace(request.RowVersion) ||
            string.IsNullOrWhiteSpace(request.Reason))
        {
            return BadRequest(
                FuelApiResponseDto<FuelAcceptanceCommandResultDto>
                .FromError(
                    "Fuel Acceptance number, row version, and reversal reason are required."));
        }


        try
        {
            var result =
                await repository.ReverseAsync(
                    fuelAcceptanceNo.Trim(),
                    request.RowVersion.Trim(),
                    request.Reason.Trim(),
                    CurrentUserId(),
                    cancellationToken);


            return Ok(
                FuelApiResponseDto<FuelAcceptanceCommandResultDto>
                .FromData(
                    result,
                    result.Message));
        }

        catch (SqlException exception)
            when (exception.Number is >= 51000 and <= 51009)
        {
            return Conflict(
                FuelApiResponseDto<FuelAcceptanceCommandResultDto>
                .FromError(
                    exception.Message));
        }

        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(
                FuelApiResponseDto<FuelAcceptanceCommandResultDto>
                .FromError(
                    exception.Message));
        }
    }


    /* ============================================================
       NEW LOOKUP:
       OPEN PO LINES BY SUPPLIER

       Example:

       GET
       /api/fuel-acceptances/lookups/open-po-lines
           ?suppCode=RICH01
           &excludeAcceptanceNo=FCA-260915-000001
           &search=066239

       ============================================================ */

    [HttpGet("lookups/open-po-lines")]
    public async Task<
        ActionResult<
            FuelApiResponseDto<
                IReadOnlyList<FuelPoLineLookupDto>>>>
        OpenPoLines(
            [FromQuery] string suppCode,
            [FromQuery] string? excludeAcceptanceNo,
            [FromQuery] string? search,
            CancellationToken cancellationToken)
    {
        if (
            string.IsNullOrWhiteSpace(
                suppCode))
        {
            return BadRequest(
                FuelApiResponseDto<
                    IReadOnlyList<FuelPoLineLookupDto>>
                .FromError(
                    "Supplier Code is required."));
        }


        try
        {
            var result =
                await repository.GetOpenPoLinesAsync(
                    suppCode.Trim(),
                    Clean(excludeAcceptanceNo),
                    Clean(search),
                    cancellationToken);


            return Ok(
                FuelApiResponseDto<
                    IReadOnlyList<FuelPoLineLookupDto>>
                .FromData(result));
        }

        catch (SqlException exception)
            when (exception.Number is >= 51000 and <= 51009)
        {
            return BadRequest(
                FuelApiResponseDto<
                    IReadOnlyList<FuelPoLineLookupDto>>
                .FromError(
                    exception.Message));
        }
    }


    /* ============================================================
       SUPPLIERS
       ============================================================ */

    [HttpGet("lookups/suppliers")]
    public async Task<
        ActionResult<
            FuelApiResponseDto<
                IReadOnlyList<FuelSupplierLookupDto>>>>
        Suppliers(
            [FromQuery] string? search,
            CancellationToken cancellationToken)
    {
        var result =
            await repository.GetSuppliersAsync(
                search,
                cancellationToken);


        return Ok(
            FuelApiResponseDto<
                IReadOnlyList<FuelSupplierLookupDto>>
            .FromData(result));
    }


    /* ============================================================
       BRANCHES
       ============================================================ */

    [HttpGet("lookups/branches")]
    public async Task<
        ActionResult<
            FuelApiResponseDto<
                IReadOnlyList<FuelBranchLookupDto>>>>
        Branches(
            [FromQuery] string? search,
            CancellationToken cancellationToken)
    {
        var result =
            await repository.GetBranchesAsync(
                search,
                cancellationToken);


        return Ok(
            FuelApiResponseDto<
                IReadOnlyList<FuelBranchLookupDto>>
            .FromData(result));
    }


    /* ============================================================
       TAX SCHEMES
       ============================================================ */

    [HttpGet("lookups/tax-schemes")]
    public async Task<
        ActionResult<
            FuelApiResponseDto<
                IReadOnlyList<FuelTaxSchemeLookupDto>>>>
        TaxSchemes(
            CancellationToken cancellationToken)
    {
        var result =
            await repository.GetTaxSchemesAsync(
                cancellationToken);


        return Ok(
            FuelApiResponseDto<
                IReadOnlyList<FuelTaxSchemeLookupDto>>
            .FromData(result));
    }


    /* ============================================================
       OPEN FOR VOUCHER PAYABLE
       ============================================================ */

    [HttpGet("open-for-vp")]
    public async Task<
        ActionResult<
            FuelApiResponseDto<
                IReadOnlyList<FuelVpSourceDto>>>>
        OpenForVp(
            [FromQuery] string? supplierCode,
            [FromQuery] string? branchName,
            [FromQuery] string? search,
            CancellationToken cancellationToken)
    {
        var result =
            await repository.GetOpenForVpAsync(
                supplierCode,
                branchName,
                search,
                cancellationToken);


        return Ok(
            FuelApiResponseDto<
                IReadOnlyList<FuelVpSourceDto>>
            .FromData(result));
    }


    /* ============================================================
       CURRENT USER
       ============================================================ */

    private string CurrentUserId() =>
        User.FindFirstValue(
            ClaimTypes.NameIdentifier)
        ??
        User.FindFirstValue(
            "user_id")
        ??
        throw new UnauthorizedAccessException(
            "Authenticated user identifier is missing.");


    private static string? Clean(
        string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}