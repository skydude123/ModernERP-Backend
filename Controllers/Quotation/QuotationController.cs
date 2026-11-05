
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using HRIStwinbeeAPI.DTO.Quotation;
using HRIStwinbeeAPI.Services.Quotation;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace HRIStwinbeeAPI.Controllers.Quotation;

[ApiController]
[Route("api/quotation")]
[Authorize(AuthenticationSchemes = "QuotationCookie")]
public sealed class QuotationController : ControllerBase
{
    private readonly IQuotationService _service;
    private readonly ILogger<QuotationController> _logger;

    public QuotationController(
        IQuotationService service,
        ILogger<QuotationController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // ==========================================================
    // QUOTATION PERMISSION
    // ==========================================================

    private bool IsQuotationUser()
    {
        return User.Identity?.IsAuthenticated == true

            && User.HasClaim(
                "application",
                "TPAQUOTATION")

            && User.HasClaim(
                "erp_permission",
                "QUOTATION");
    }

    // ==========================================================
    // VALIDATE SQ NUMBER
    // ==========================================================

    private static bool InvalidSqNo(string? sqNo)
    {
        var value = sqNo?.Trim();

        return string.IsNullOrWhiteSpace(value) ||
               value.Length != 10 ||
               !value.All(char.IsDigit);
    }

    // ==========================================================
    // GET COMPLETE QUOTATION
    // GET /api/quotation/{sqNo}
    // ==========================================================

    [HttpGet("{sqNo}")]
    public async Task<IActionResult> GetQuotation(
        string sqNo,
        CancellationToken cancellationToken)
    {
        if (!IsQuotationUser())
        {
            return Forbid();
        }

        if (InvalidSqNo(sqNo))
        {
            return BadRequest(new
            {
                message = "Invalid quotation number."
            });
        }

        var result = await _service.GetQuotationAsync(
            sqNo,
            cancellationToken);

        return result is null
            ? NotFound(new
            {
                message = "Quotation was not found."
            })
            : Ok(result);
    }

    // ==========================================================
    // GET HEADER
    // GET /api/quotation/{sqNo}/header
    // ==========================================================

    [HttpGet("{sqNo}/header")]
    public async Task<IActionResult> GetHeader(
        string sqNo,
        CancellationToken cancellationToken)
    {
        if (!IsQuotationUser())
        {
            return Forbid();
        }

        if (InvalidSqNo(sqNo))
        {
            return BadRequest(new
            {
                message = "Invalid quotation number."
            });
        }

        var result = await _service.GetHeaderAsync(
            sqNo,
            cancellationToken);

        return result is null
            ? NotFound()
            : Ok(result);
    }

    // ==========================================================
    // GET DETAILS
    // GET /api/quotation/{sqNo}/details
    // ==========================================================

    [HttpGet("{sqNo}/details")]
    public async Task<IActionResult> GetDetails(
        string sqNo,
        CancellationToken cancellationToken)
    {
        if (!IsQuotationUser())
        {
            return Forbid();
        }

        if (InvalidSqNo(sqNo))
        {
            return BadRequest(new
            {
                message = "Invalid quotation number."
            });
        }

        try
        {
            var result = await _service.GetDetailsAsync(
                sqNo,
                cancellationToken);

            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new
            {
                message = "Quotation was not found."
            });
        }
    }

    // ==========================================================
    // GET ADDITIONAL CHARGES
    // GET /api/quotation/{sqNo}/additional-charges
    // ==========================================================

    [HttpGet("{sqNo}/additional-charges")]
    public async Task<IActionResult> GetAdditionalCharges(
        string sqNo,
        CancellationToken cancellationToken)
    {
        if (!IsQuotationUser())
        {
            return Forbid();
        }

        if (InvalidSqNo(sqNo))
        {
            return BadRequest(new
            {
                message = "Invalid quotation number."
            });
        }

        var result =
            await _service.GetAdditionalChargesAsync(
                sqNo,
                cancellationToken);

        return result is null
            ? NotFound()
            : Ok(result);
    }

    // ==========================================================
    // GET TERMS
    // GET /api/quotation/lookups/terms
    // ==========================================================

    [HttpGet("lookups/terms")]
    public async Task<IActionResult> GetTerms(
        CancellationToken cancellationToken)
    {
        if (!IsQuotationUser())
        {
            return Forbid();
        }

        var result = await _service.GetTermsAsync(
            cancellationToken);

        return Ok(result);
    }

    // ==========================================================
    // GET ARTISTS
    // GET /api/quotation/lookups/artists
    // ==========================================================

    [HttpGet("lookups/artists")]
    public async Task<IActionResult> GetArtists(
        [FromQuery] string? branchName,
        CancellationToken cancellationToken)
    {
        if (!IsQuotationUser())
        {
            return Forbid();
        }

        var authenticatedBranch =
            User.FindFirstValue("branch");

        var isAdmin = string.Equals(
            User.FindFirstValue("administrator"),
            "Y",
            StringComparison.OrdinalIgnoreCase);

        // Non-admin cannot select another branch.
        var effectiveBranch =
            isAdmin && !string.IsNullOrWhiteSpace(branchName)
                ? branchName.Trim()
                : authenticatedBranch?.Trim();

        if (string.IsNullOrWhiteSpace(effectiveBranch) ||
            effectiveBranch.Length > 10)
        {
            return BadRequest(new
            {
                message = "A valid branch is required."
            });
        }

        var result = await _service.GetArtistsAsync(
            effectiveBranch,
            cancellationToken);

        return Ok(result);
    }

    // ==========================================================
    // GET CUSTOMERS
    // GET /api/quotation/lookups/customers
    // ==========================================================

    [HttpGet("lookups/customers")]
    public async Task<IActionResult> GetCustomers(
        CancellationToken cancellationToken)
    {
        if (!IsQuotationUser())
        {
            return Forbid();
        }

        var result = await _service.GetCustomersAsync(
            cancellationToken);

        return Ok(result);
    }

    // ==========================================================
    // GET QUOTATION LIST
    // GET /api/quotation/list
    // ==========================================================

    [HttpGet("list")]
    public async Task<IActionResult> GetList(
        CancellationToken cancellationToken)
    {
        if (!IsQuotationUser())
        {
            return Forbid();
        }

        var userId =
            User.FindFirstValue("user_id");

        var branchName =
            User.FindFirstValue("branch");

        if (string.IsNullOrWhiteSpace(userId) ||
            string.IsNullOrWhiteSpace(branchName))
        {
            return Unauthorized(new
            {
                message =
                    "Quotation user or branch is missing."
            });
        }

        var result = await _service.GetListAsync(
            userId,
            branchName,
            cancellationToken);

        return Ok(result);
    }

    // ==========================================================
    // GET UNIT OF MEASURE
    // GET /api/quotation/lookups/um
    // ==========================================================

    [HttpGet("lookups/um")]
    public async Task<IActionResult> GetUnitsOfMeasure(
        CancellationToken cancellationToken)
    {
        if (!IsQuotationUser())
        {
            return Forbid();
        }

        var result =
            await _service.GetUnitsOfMeasureAsync(
                cancellationToken);

        return Ok(result);
    }

    // ==========================================================
    // GET WARRANTY
    // GET /api/quotation/lookups/warranty
    // ==========================================================

    [HttpGet("lookups/warranty")]
    public async Task<IActionResult> GetWarranty(
        CancellationToken cancellationToken)
    {
        if (!IsQuotationUser())
        {
            return Forbid();
        }

        var result =
            await _service.GetWarrantyAsync(
                cancellationToken);

        return Ok(result);
    }

    // ==========================================================
    // GET DELIVERY
    // GET /api/quotation/lookups/delivery
    // ==========================================================

    [HttpGet("lookups/delivery")]
    public async Task<IActionResult> GetDelivery(
        CancellationToken cancellationToken)
    {
        if (!IsQuotationUser())
        {
            return Forbid();
        }

        var result =
            await _service.GetDeliveryAsync(
                cancellationToken);

        return Ok(result);
    }


    // ==========================================================
    // GET SERVICE LOOKUP
    // GET /api/quotation/lookups/services
    // ==========================================================

    [HttpGet("lookups/services")]
    public async Task<IActionResult> GetServices(
        CancellationToken cancellationToken)
    {
        if (!IsQuotationUser())
        {
            return Forbid();
        }

        var result = await _service.GetServicesAsync(
            cancellationToken);

        return Ok(result);
    }


    // ==========================================================
    // CONFIRM QUOTATION
    // POST /api/quotation/{sqNo}/confirm
    // ==========================================================

    [HttpPost("{sqNo}/confirm")]
    public async Task<IActionResult> ConfirmQuotation(
        string sqNo,
        [FromBody] QuotationConfirmRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!IsQuotationUser())
        {
            return Forbid();
        }

        if (InvalidSqNo(sqNo))
        {
            return BadRequest(new
            {
                message = "Invalid quotation number."
            });
        }

        if (request is null)
        {
            return BadRequest(new
            {
                message = "Confirmation information is required."
            });
        }

        // Values are obtained from the authenticated cookie.
        // Never accept actor identity from the request body.

        var userId =
            User.FindFirstValue("user_id")?.Trim();

        var branchName =
            User.FindFirstValue("branch")?.Trim();

        var isAdministrator = string.Equals(
            User.FindFirstValue("administrator"),
            "Y",
            StringComparison.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(userId) ||
            (!isAdministrator &&
             string.IsNullOrWhiteSpace(branchName)))
        {
            return Unauthorized(new
            {
                message = "Quotation session is invalid."
            });
        }

        try
        {
            var result = await _service.ConfirmAsync(
                sqNo,
                request,
                userId,
                branchName ?? "",
                isAdministrator,
                cancellationToken);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (SqlException ex) when
            (ex.Number >= 51001 && ex.Number <= 51008)
        {
            _logger.LogWarning(
                ex,
                "Quotation confirmation rejected. SqNo={SqNo}, User={UserId}",
                sqNo,
                userId);

            var message = ex.Number switch
            {
                51001 => "Quotation number is required.",
                51002 => "Confirmation reference is required.",
                51003 => "Confirmation date is required.",
                51004 => "Authenticated user is required.",
                51005 => "Authenticated branch is required.",
                51006 => "Quotation not found or access denied.",
                51007 => "Only NEW quotations can be confirmed.",
                51008 => "Quotation confirmation failed.",
                _ => "Confirmation was rejected."
            };

            return Conflict(new
            {
                message
            });
        }
        catch (SqlException ex)
        {
            _logger.LogError(
                ex,
                "Quotation confirmation SQL error. SqNo={SqNo}, User={UserId}",
                sqNo,
                userId);

            return StatusCode(500, new
            {
                message =
                    "Quotation confirmation failed. " +
                    "Please check the API server logs."
            });
        }
    }



    // ==========================================================
    // SINGLE SAVE ENDPOINT
    //
    // POST /api/quotation/upsert
    //
    // INSERT -> NEW QUOTATION
    // UPDATE -> EXISTING NEW QUOTATION
    // ==========================================================

    [HttpPost("upsert")]
    public async Task<IActionResult> UpsertQuotation(
        [FromBody] QuotationUpsertRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsQuotationUser())
        {
            return Forbid();
        }

        if (request is null)
        {
            return BadRequest(new
            {
                message = "Quotation request is required."
            });
        }

        var action =
            (request.Action ?? "")
            .Trim()
            .ToUpperInvariant();

        if (action != "INSERT" &&
            action != "UPDATE")
        {
            return BadRequest(new
            {
                message = "Invalid quotation action."
            });
        }

        if (action == "UPDATE" &&
            InvalidSqNo(request.SqNo))
        {
            return BadRequest(new
            {
                message = "Invalid quotation number."
            });
        }

        // ======================================================
        // AUTHENTICATED USER
        // ======================================================

        var userId =
            User.FindFirstValue("user_id");

        var branchName =
            User.FindFirstValue("branch");

        var isAdministrator = string.Equals(
            User.FindFirstValue("administrator"),
            "Y",
            StringComparison.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(userId) ||
            string.IsNullOrWhiteSpace(branchName))
        {
            return Unauthorized(new
            {
                message = "Quotation session is invalid."
            });
        }


        // ======================================================
        // SALESPERSON AUTO-FILL
        // INSERT ONLY - FROM AUTHENTICATED SESSION
        // ======================================================

        if (action == "INSERT")
        {
            var salespersonFullName =
                (User.Identity?.Name ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(salespersonFullName))
            {
                return BadRequest(new
                {
                    message =
                        "Salesperson full name is missing from " +
                        "the authenticated session."
                });
            }

            // TWINBEE.dbo.QUOTATIONHDR.acperson VARCHAR(30)
            if (salespersonFullName.Length > 30)
            {
                return BadRequest(new
                {
                    message =
                        "Salesperson full name exceeds the " +
                        "30-character database limit."
                });
            }

            // Set the value on the trusted server side.
            // Ignore any AcPerson supplied by the frontend.
            request.AcPerson = salespersonFullName;
        }


        try
        {
            // ==================================================
            // ONE SERVICE METHOD
            // ==================================================

            var result = await _service.UpsertAsync(
                request,
                userId,
                branchName,
                isAdministrator,
                cancellationToken);

            if (!result.Success)
            {
                return Conflict(new
                {
                    message =
                        result.Message ??
                        "Quotation save was not successful."
                });
            }

            // ==================================================
            // INSERT SUCCESS
            // ==================================================

            if (action == "INSERT")
            {
                return CreatedAtAction(
                    nameof(GetQuotation),
                    new
                    {
                        sqNo = result.SqNo.Trim()
                    },
                    result);
            }

            // ==================================================
            // UPDATE SUCCESS
            // ==================================================

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
        catch (SqlException ex)
        {
            // Log SQL error on the server.
            // Do not expose database details to users.

            _logger.LogError(
                ex,
                "Quotation UPSERT SQL error. Action={Action}, User={UserId}",
                action,
                userId);

            return Conflict(new
            {
                message =
                    "Quotation save was rejected by the database. " +
                    "Please check the API server logs."
            });
        }
    }
}
