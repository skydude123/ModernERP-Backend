using System.Security.Claims;
using HRIStwinbeeAPI.DTO.ServiceAcceptance;
using HRIStwinbeeAPI.Repo.Interface.ServiceAcceptance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRIStwinbeeAPI.Controllers.ServiceAcceptance;

[ApiController]
[Authorize]
[Route("api/service-acceptances")]
public sealed class ServiceAcceptancesController(
    IServiceAcceptanceRepository repository)
    : ControllerBase
{
    /* ============================================================
       LIST
       ============================================================ */

    [HttpGet]
    public async Task<
        ActionResult<
            ServiceAcceptanceApiResponseDto<
                ServiceAcceptancePagedResultDto<
                    ServiceAcceptanceListItemDto>>>>
        Query(
            [FromQuery]
            ServiceAcceptanceQueryDto query,
            CancellationToken token)
    {
        try
        {
            var result =
                await repository
                    .QueryAsync(
                        query,
                        token);

            return Ok(
                ServiceAcceptanceApiResponseDto<
                    ServiceAcceptancePagedResultDto<
                        ServiceAcceptanceListItemDto>>
                    .FromData(
                        result));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                ServiceAcceptanceApiResponseDto<
                    ServiceAcceptancePagedResultDto<
                        ServiceAcceptanceListItemDto>>
                    .FromError(
                        ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(
                ServiceAcceptanceApiResponseDto<
                    ServiceAcceptancePagedResultDto<
                        ServiceAcceptanceListItemDto>>
                    .FromError(
                        ex.Message));
        }
    }


    /* ============================================================
       DOCUMENT
       ============================================================ */

    [HttpGet("{documentNo}")]
    public async Task<
        ActionResult<
            ServiceAcceptanceApiResponseDto<
                ServiceAcceptanceDocumentDto>>>
        Get(
            [FromRoute]
            string documentNo,
            CancellationToken token)
    {
        try
        {
            if (
                string.IsNullOrWhiteSpace(
                    documentNo)
            )
            {
                return BadRequest(
                    ServiceAcceptanceApiResponseDto<
                        ServiceAcceptanceDocumentDto>
                    .FromError(
                        "Service Acceptance number is required."));
            }

            var result =
                await repository
                    .GetAsync(
                        documentNo.Trim(),
                        token);

            if (result is null)
            {
                return NotFound(
                    ServiceAcceptanceApiResponseDto<
                        ServiceAcceptanceDocumentDto>
                    .FromError(
                        "Service Acceptance was not found."));
            }

            return Ok(
                ServiceAcceptanceApiResponseDto<
                    ServiceAcceptanceDocumentDto>
                .FromData(
                    result));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                ServiceAcceptanceApiResponseDto<
                    ServiceAcceptanceDocumentDto>
                .FromError(
                    ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(
                ServiceAcceptanceApiResponseDto<
                    ServiceAcceptanceDocumentDto>
                .FromError(
                    ex.Message));
        }
    }


    /* ============================================================
       SAVE
       ============================================================ */

    [HttpPost]
    public async Task<
        ActionResult<
            ServiceAcceptanceApiResponseDto<
                ServiceAcceptanceCommandResultDto>>>
        Save(
            [FromBody]
            ServiceAcceptanceSaveDto request,
            CancellationToken token)
    {
        try
        {
            if (request is null)
            {
                return BadRequest(
                    ServiceAcceptanceApiResponseDto<
                        ServiceAcceptanceCommandResultDto>
                    .FromError(
                        "Request body is required."));
            }

            if (request.Header is null)
            {
                return BadRequest(
                    ServiceAcceptanceApiResponseDto<
                        ServiceAcceptanceCommandResultDto>
                    .FromError(
                        "Service Acceptance header is required."));
            }

            /*
             * Never trust userId coming from Angular.
             */
            request.Header.UserId =
                CurrentUserId();

            var result =
                await repository
                    .SaveAsync(
                        request,
                        token);

            return Ok(
                ServiceAcceptanceApiResponseDto<
                    ServiceAcceptanceCommandResultDto>
                .FromData(
                    result,
                    result.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(
                ServiceAcceptanceApiResponseDto<
                    ServiceAcceptanceCommandResultDto>
                .FromError(
                    ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                ServiceAcceptanceApiResponseDto<
                    ServiceAcceptanceCommandResultDto>
                .FromError(
                    ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(
                ServiceAcceptanceApiResponseDto<
                    ServiceAcceptanceCommandResultDto>
                .FromError(
                    ex.Message));
        }
    }


    /* ============================================================
       WORKFLOW

       New normal flow:
         ACCEPT
         POST
         REVERSE

       Submit remains accepted by the route only for compatibility.
       ============================================================ */

    [HttpPost(
        "{documentNo}/{workflowAction:regex(^(submit|accept|post|reverse)$)}")]
    public async Task<
        ActionResult<
            ServiceAcceptanceApiResponseDto<
                ServiceAcceptanceCommandResultDto>>>
        Transition(
            [FromRoute]
            string documentNo,
            [FromRoute]
            string workflowAction,
            [FromBody]
            ServiceAcceptanceWorkflowDto request,
            CancellationToken token)
    {
        try
        {
            if (
                string.IsNullOrWhiteSpace(
                    documentNo)
            )
            {
                return BadRequest(
                    ServiceAcceptanceApiResponseDto<
                        ServiceAcceptanceCommandResultDto>
                    .FromError(
                        "Document number is required."));
            }

            if (request is null)
            {
                return BadRequest(
                    ServiceAcceptanceApiResponseDto<
                        ServiceAcceptanceCommandResultDto>
                    .FromError(
                        "Workflow request body is required."));
            }

            if (
                string.IsNullOrWhiteSpace(
                    request.RowVersion)
            )
            {
                return BadRequest(
                    ServiceAcceptanceApiResponseDto<
                        ServiceAcceptanceCommandResultDto>
                    .FromError(
                        "Row version is required."));
            }

            var normalizedAction =
                workflowAction
                    .Trim()
                    .ToUpperInvariant();

            if (
                normalizedAction ==
                "REVERSE"

                &&

                string.IsNullOrWhiteSpace(
                    request.Reason)
            )
            {
                return BadRequest(
                    ServiceAcceptanceApiResponseDto<
                        ServiceAcceptanceCommandResultDto>
                    .FromError(
                        "Reversal reason is required."));
            }

            request.UserId =
                CurrentUserId();

            var result =
                await repository
                    .TransitionAsync(
                        documentNo.Trim(),
                        normalizedAction,
                        request,
                        token);

            return Ok(
                ServiceAcceptanceApiResponseDto<
                    ServiceAcceptanceCommandResultDto>
                .FromData(
                    result,
                    result.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(
                ServiceAcceptanceApiResponseDto<
                    ServiceAcceptanceCommandResultDto>
                .FromError(
                    ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                ServiceAcceptanceApiResponseDto<
                    ServiceAcceptanceCommandResultDto>
                .FromError(
                    ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(
                ServiceAcceptanceApiResponseDto<
                    ServiceAcceptanceCommandResultDto>
                .FromError(
                    ex.Message));
        }
    }


    /* ============================================================
       OPEN PO LINES BY SUPPLIER
       ============================================================ */

    [HttpGet("lookups/open-po-lines")]
    public async Task<
        ActionResult<
            ServiceAcceptanceApiResponseDto<
                IReadOnlyList<
                    ServiceOpenPoLineLookupDto>>>>
        OpenPoLines(
            [FromQuery]
            string suppCode,
            [FromQuery]
            string? branchName,
            [FromQuery]
            string? excludeAcceptanceNo,
            [FromQuery]
            string? search,
            CancellationToken token)
    {
        try
        {
            if (
                string.IsNullOrWhiteSpace(
                    suppCode)
            )
            {
                return BadRequest(
                    ServiceAcceptanceApiResponseDto<
                        IReadOnlyList<
                            ServiceOpenPoLineLookupDto>>
                    .FromError(
                        "Supplier code is required."));
            }

            var result =
                await repository
                    .GetOpenPoLinesAsync(
                        suppCode.Trim(),
                        branchName,
                        excludeAcceptanceNo,
                        search,
                        token);

            return Ok(
                ServiceAcceptanceApiResponseDto<
                    IReadOnlyList<
                        ServiceOpenPoLineLookupDto>>
                .FromData(
                    result));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                ServiceAcceptanceApiResponseDto<
                    IReadOnlyList<
                        ServiceOpenPoLineLookupDto>>
                .FromError(
                    ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(
                ServiceAcceptanceApiResponseDto<
                    IReadOnlyList<
                        ServiceOpenPoLineLookupDto>>
                .FromError(
                    ex.Message));
        }
    }


    /* ============================================================
       SUPPLIERS
       ============================================================ */

    [HttpGet("lookups/suppliers")]
    public async Task<
        ActionResult<
            ServiceAcceptanceApiResponseDto<
                IReadOnlyList<
                    ServiceSupplierLookupDto>>>>
        Suppliers(
            [FromQuery]
            string? search,
            CancellationToken token)
    {
        try
        {
            var result =
                await repository
                    .GetSuppliersAsync(
                        search,
                        token);

            return Ok(
                ServiceAcceptanceApiResponseDto<
                    IReadOnlyList<
                        ServiceSupplierLookupDto>>
                .FromData(
                    result));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                ServiceAcceptanceApiResponseDto<
                    IReadOnlyList<
                        ServiceSupplierLookupDto>>
                .FromError(
                    ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(
                ServiceAcceptanceApiResponseDto<
                    IReadOnlyList<
                        ServiceSupplierLookupDto>>
                .FromError(
                    ex.Message));
        }
    }


    /* ============================================================
       BRANCHES
       ============================================================ */

    [HttpGet("lookups/branches")]
    public async Task<
        ActionResult<
            ServiceAcceptanceApiResponseDto<
                IReadOnlyList<
                    ServiceBranchLookupDto>>>>
        Branches(
            CancellationToken token)
    {
        try
        {
            var result =
                await repository
                    .GetBranchesAsync(
                        token);

            return Ok(
                ServiceAcceptanceApiResponseDto<
                    IReadOnlyList<
                        ServiceBranchLookupDto>>
                .FromData(
                    result));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                ServiceAcceptanceApiResponseDto<
                    IReadOnlyList<
                        ServiceBranchLookupDto>>
                .FromError(
                    ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(
                ServiceAcceptanceApiResponseDto<
                    IReadOnlyList<
                        ServiceBranchLookupDto>>
                .FromError(
                    ex.Message));
        }
    }


    /* ============================================================
       TAX
       ============================================================ */

    [HttpGet("lookups/tax-schemes")]
    public async Task<
        ActionResult<
            ServiceAcceptanceApiResponseDto<
                IReadOnlyList<
                    ServiceTaxSchemeLookupDto>>>>
        TaxSchemes(
            CancellationToken token)
    {
        try
        {
            var result =
                await repository
                    .GetTaxSchemesAsync(
                        token);

            return Ok(
                ServiceAcceptanceApiResponseDto<
                    IReadOnlyList<
                        ServiceTaxSchemeLookupDto>>
                .FromData(
                    result));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                ServiceAcceptanceApiResponseDto<
                    IReadOnlyList<
                        ServiceTaxSchemeLookupDto>>
                .FromError(
                    ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(
                ServiceAcceptanceApiResponseDto<
                    IReadOnlyList<
                        ServiceTaxSchemeLookupDto>>
                .FromError(
                    ex.Message));
        }
    }


    /* ============================================================
       VP
       ============================================================ */

    [HttpGet("vp-sources")]
    public async Task<
        ActionResult<
            ServiceAcceptanceApiResponseDto<
                IReadOnlyList<
                    ServiceVpSourceDto>>>>
        VpSources(
            [FromQuery]
            string? supplierCode,
            [FromQuery]
            string? branchName,
            [FromQuery]
            string? search,
            CancellationToken token)
    {
        try
        {
            var result =
                await repository
                    .GetVpSourcesAsync(
                        supplierCode,
                        branchName,
                        search,
                        token);

            return Ok(
                ServiceAcceptanceApiResponseDto<
                    IReadOnlyList<
                        ServiceVpSourceDto>>
                .FromData(
                    result));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                ServiceAcceptanceApiResponseDto<
                    IReadOnlyList<
                        ServiceVpSourceDto>>
                .FromError(
                    ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(
                ServiceAcceptanceApiResponseDto<
                    IReadOnlyList<
                        ServiceVpSourceDto>>
                .FromError(
                    ex.Message));
        }
    }


    /* ============================================================
       CURRENT USER
       ============================================================ */

    private string CurrentUserId()
    {
        var userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)

            ?? User.FindFirstValue(
                "user_id")

            ?? User.FindFirstValue(
                "userId")

            ?? User.FindFirstValue(
                "sub");


        if (
            string.IsNullOrWhiteSpace(
                userId)
        )
        {
            throw new UnauthorizedAccessException(
                "Authenticated user identifier is missing.");
        }


        return userId.Trim();
    }
}