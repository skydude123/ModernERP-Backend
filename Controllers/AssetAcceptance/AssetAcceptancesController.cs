using HRIStwinbeeAPI.DTO.AssetAcceptance;
using HRIStwinbeeAPI.Repo.Interface.AssetAcceptance;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
//using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
namespace HRIStwinbeeAPI.Controllers.AssetAcceptance;

[ApiController]
[Route("api/asset-acceptances")]
public sealed class AssetAcceptancesController(
    IAssetAcceptanceRepository repository)
    : ControllerBase
{
    // ============================================================
    // GET LIST
    // GET: api/asset-acceptances
    // ============================================================

    [HttpGet]
    [ProducesResponseType(
        typeof(
            ApiResponseDto<
                PagedResultDto<AssetAcceptanceListItemDto>>),
        StatusCodes.Status200OK)]
    public async Task<
        ActionResult<
            ApiResponseDto<
                PagedResultDto<AssetAcceptanceListItemDto>>>>
        Query(
            [FromQuery] AssetAcceptanceQueryDto request,
            CancellationToken cancellationToken)
    {
        var result =
            await repository.QueryAsync(
                request,
                cancellationToken);

        return Ok(
            ApiResponseDto<
                PagedResultDto<AssetAcceptanceListItemDto>>
                .FromData(result));
    }

    // ============================================================
    // GET SINGLE DOCUMENT
    // GET: api/asset-acceptances/{documentNo}
    // ============================================================

    [HttpGet("{documentNo}")]
    [ProducesResponseType(
        typeof(ApiResponseDto<AssetAcceptanceDocumentDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponseDto<AssetAcceptanceDocumentDto>),
        StatusCodes.Status404NotFound)]
    public async Task<
        ActionResult<
            ApiResponseDto<AssetAcceptanceDocumentDto>>>
        Get(
            [FromRoute] string documentNo,
            CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(documentNo))
        {
            return BadRequest(
                ApiResponseDto<AssetAcceptanceDocumentDto>
                    .FromError(
                        "Asset Acceptance Number is required."));
        }

        var result =
            await repository.GetAsync(
                documentNo.Trim(),
                cancellationToken);

        if (result is null)
        {
            return NotFound(
                ApiResponseDto<AssetAcceptanceDocumentDto>
                    .FromError(
                        "Asset acceptance was not found."));
        }

        return Ok(
            ApiResponseDto<AssetAcceptanceDocumentDto>
                .FromData(result));
    }

    // ============================================================
    // CREATE
    // POST: api/asset-acceptances
    // ============================================================

    //[HttpPost]
    //[ProducesResponseType(
    //    typeof(
    //        ApiResponseDto<AssetAcceptanceCommandResultDto>),
    //    StatusCodes.Status200OK)]
    //[ProducesResponseType(
    //    typeof(
    //        ApiResponseDto<AssetAcceptanceCommandResultDto>),
    //    StatusCodes.Status400BadRequest)]
    //public async Task<
    //    ActionResult<
    //        ApiResponseDto<AssetAcceptanceCommandResultDto>>>
    //    Create(
    //        [FromBody] AssetAcceptanceSaveDto request,
    //        CancellationToken cancellationToken)
    //{
    //    if (request is null)
    //    {
    //        return BadRequest(
    //            ApiResponseDto<AssetAcceptanceCommandResultDto>
    //                .FromError(
    //                    "Asset acceptance request is required."));
    //    }

    //    request.Header.AssetAcceptanceNo = null;
    //    request.Header.RowVersion = null;

    //    var result =
    //        await repository.UpsertAsync(
    //            request,
    //            cancellationToken);

    //    return Ok(
    //        ApiResponseDto<AssetAcceptanceCommandResultDto>
    //            .FromData(
    //                result,
    //                result.Message));
    //}
    [HttpPost]
    [ProducesResponseType(
        typeof(ApiResponseDto<AssetAcceptanceCommandResultDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponseDto<AssetAcceptanceCommandResultDto>),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ApiResponseDto<AssetAcceptanceCommandResultDto>),
        StatusCodes.Status409Conflict)]
    [ProducesResponseType(
        typeof(ApiResponseDto<AssetAcceptanceCommandResultDto>),
        StatusCodes.Status500InternalServerError)]
    public async Task<
        ActionResult<
            ApiResponseDto<AssetAcceptanceCommandResultDto>>>
        Upsert(
            [FromBody] AssetAcceptanceSaveDto request,
            CancellationToken cancellationToken)
    {
        /*
         * Validate the request object.
         */
        if (request is null)
        {
            return BadRequest(
                ApiResponseDto<AssetAcceptanceCommandResultDto>
                    .FromError(
                        "Asset acceptance request is required."));
        }

        /*
         * Validate the request header.
         */
        if (request.Header is null)
        {
            return BadRequest(
                ApiResponseDto<AssetAcceptanceCommandResultDto>
                    .FromError(
                        "Asset acceptance header is required."));
        }

        /*
         * Clean the document number.
         *
         * Empty or whitespace means CREATE.
         * A value means UPDATE.
         */
        request.Header.AssetAcceptanceNo =
            string.IsNullOrWhiteSpace(
                request.Header.AssetAcceptanceNo)
                ? null
                : request.Header
                    .AssetAcceptanceNo
                    .Trim();

        /*
         * Clean the row version.
         */
        request.Header.RowVersion =
            string.IsNullOrWhiteSpace(
                request.Header.RowVersion)
                ? null
                : request.Header
                    .RowVersion
                    .Trim();

        var isUpdate =
            request.Header.AssetAcceptanceNo is not null;

        /*
         * UPDATE requires the exact current row version
         * returned by the database.
         */
        if (isUpdate &&
            request.Header.RowVersion is null)
        {
            return BadRequest(
                ApiResponseDto<AssetAcceptanceCommandResultDto>
                    .FromError(
                        "Row Version is required when updating an existing Asset Acceptance."));
        }

        /*
         * CREATE must not carry a row version.
         */
        if (!isUpdate)
        {
            request.Header.RowVersion = null;
        }

        try
        {
            var result =
                await repository.UpsertAsync(
                    request,
                    cancellationToken);

            return Ok(
                ApiResponseDto<AssetAcceptanceCommandResultDto>
                    .FromData(
                        result,
                        result.Message));
        }
        /*
         * Handles custom errors raised by the stored procedure
         * using THROW or RAISERROR with error number 50000+.
         *
         * Examples:
         * - Row Version mismatch
         * - Invalid PO
         * - Invalid document status
         * - Duplicate asset acceptance
         * - Missing required database information
         */
        catch (SqlException exception)
            when (exception.Number >= 50000)
        {
            return Conflict(
                ApiResponseDto<AssetAcceptanceCommandResultDto>
                    .FromError(
                        exception.Message));
        }
        /*
         * Handles normal SQL Server/database errors.
         *
         * Examples:
         * - Missing table or column
         * - Foreign-key violation
         * - Conversion error
         * - SQL timeout
         * - Database connection error
         */
        catch (SqlException exception)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponseDto<AssetAcceptanceCommandResultDto>
                    .FromError(
                        $"A database error occurred while saving the Asset Acceptance. " +
                        $"SQL Error {exception.Number}: {exception.Message}"));
        }
        /*
         * Handles a cancelled HTTP request.
         */
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            return StatusCode(
                StatusCodes.Status499ClientClosedRequest,
                ApiResponseDto<AssetAcceptanceCommandResultDto>
                    .FromError(
                        "The Asset Acceptance save request was cancelled."));
        }
        /*
         * Handles all other unexpected errors.
         */
        catch (Exception exception)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponseDto<AssetAcceptanceCommandResultDto>
                    .FromError(
                        $"An unexpected error occurred while saving the Asset Acceptance: " +
                        exception.Message));
        }
    }

    // ============================================================
    // UPDATE
    // PUT: api/asset-acceptances/{documentNo}
    // ============================================================

    [HttpPut("{documentNo}")]
    [ProducesResponseType(
        typeof(
            ApiResponseDto<AssetAcceptanceCommandResultDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(
            ApiResponseDto<AssetAcceptanceCommandResultDto>),
        StatusCodes.Status400BadRequest)]
    public async Task<
        ActionResult<
            ApiResponseDto<AssetAcceptanceCommandResultDto>>>
        Update(
            [FromRoute] string documentNo,
            [FromBody] AssetAcceptanceSaveDto request,
            CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(documentNo))
        {
            return BadRequest(
                ApiResponseDto<AssetAcceptanceCommandResultDto>
                    .FromError(
                        "Asset Acceptance Number is required."));
        }

        if (request is null)
        {
            return BadRequest(
                ApiResponseDto<AssetAcceptanceCommandResultDto>
                    .FromError(
                        "Asset acceptance request is required."));
        }

        request.Header.AssetAcceptanceNo =
            documentNo.Trim();

        var result =
            await repository.UpsertAsync(
                request,
                cancellationToken);

        return Ok(
            ApiResponseDto<AssetAcceptanceCommandResultDto>
                .FromData(
                    result,
                    result.Message));
    }

    // ============================================================
    // SUBMIT
    // POST: api/asset-acceptances/{documentNo}/submit
    // ============================================================

    [HttpPost("{documentNo}/submit")]
    public async Task<
    ActionResult<
        ApiResponseDto<AssetAcceptanceCommandResultDto>>>
    Submit(
        [FromRoute] string documentNo,
        [FromBody] WorkflowRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            return await ExecuteTransition(
                documentNo,
                "SUBMIT",
                request,
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            // Preserve the cancellation behavior of ASP.NET Core.
            throw;
        }
    }

    // ============================================================
    // ACCEPT
    // POST: api/asset-acceptances/{documentNo}/accept
    // ============================================================

    [HttpPost("{documentNo}/accept")]
    public Task<
        ActionResult<
            ApiResponseDto<AssetAcceptanceCommandResultDto>>>
        Accept(
            [FromRoute] string documentNo,
            [FromBody] WorkflowRequestDto request,
            CancellationToken cancellationToken)
    {
        return ExecuteTransition(
            documentNo,
            "ACCEPT",
            request,
            cancellationToken);
    }

    // ============================================================
    // POST
    // POST: api/asset-acceptances/{documentNo}/post
    // ============================================================

    [HttpPost("{documentNo}/post")]
    public Task<
        ActionResult<
            ApiResponseDto<AssetAcceptanceCommandResultDto>>>
        Post(
            [FromRoute] string documentNo,
            [FromBody] WorkflowRequestDto request,
            CancellationToken cancellationToken)
    {
        return ExecuteTransition(
            documentNo,
            "POST",
            request,
            cancellationToken);
    }

    // ============================================================
    // CANCEL
    // POST: api/asset-acceptances/{documentNo}/cancel
    // ============================================================

    [HttpPost("{documentNo}/cancel")]
    public Task<
        ActionResult<
            ApiResponseDto<AssetAcceptanceCommandResultDto>>>
        Cancel(
            [FromRoute] string documentNo,
            [FromBody] WorkflowRequestDto request,
            CancellationToken cancellationToken)
    {
        return ExecuteTransition(
            documentNo,
            "CANCEL",
            request,
            cancellationToken);
    }

    // ============================================================
    // DELETE DRAFT
    // POST: api/asset-acceptances/{documentNo}/delete
    // ============================================================

    [HttpPost("{documentNo}/delete")]
    [ProducesResponseType(
        typeof(
            ApiResponseDto<AssetAcceptanceCommandResultDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(
            ApiResponseDto<AssetAcceptanceCommandResultDto>),
        StatusCodes.Status400BadRequest)]
    public async Task<
        ActionResult<
            ApiResponseDto<AssetAcceptanceCommandResultDto>>>
        Delete(
            [FromRoute] string documentNo,
            [FromBody] WorkflowRequestDto request,
            CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(documentNo))
        {
            return BadRequest(
                ApiResponseDto<AssetAcceptanceCommandResultDto>
                    .FromError(
                        "Asset Acceptance Number is required."));
        }

        if (request is null)
        {
            return BadRequest(
                ApiResponseDto<AssetAcceptanceCommandResultDto>
                    .FromError(
                        "Workflow request is required."));
        }

        var result =
            await repository.DeleteAsync(
                documentNo.Trim(),
                request,
                cancellationToken);

        return Ok(
            ApiResponseDto<AssetAcceptanceCommandResultDto>
                .FromData(
                    result,
                    result.Message));
    }

    // ============================================================
    // FIXED ASSET PO LOOKUP
    // GET: api/asset-acceptances/lookups/fixed-asset-pos
    // ============================================================

    [HttpGet("lookups/fixed-asset-pos")]
    public async Task<
        ActionResult<
            ApiResponseDto<
                IReadOnlyList<FixedAssetPoLookupDto>>>>
        GetFixedAssetPos(
            [FromQuery] string? search,
            [FromQuery] string? excludeAcceptanceNo,
            CancellationToken cancellationToken)
    {
        var result =
            await repository.GetFixedAssetPosAsync(
                search,
                excludeAcceptanceNo,
                cancellationToken);

        return Ok(
            ApiResponseDto<
                IReadOnlyList<FixedAssetPoLookupDto>>
                .FromData(result));
    }

    // ============================================================
    // FIXED ASSET PO LINES LOOKUP
    // GET:
    // api/asset-acceptances/lookups/fixed-asset-pos/{poNo}/lines
    // ============================================================

    [HttpGet("lookups/fixed-asset-pos/{poNo}/lines")]
    public async Task<
        ActionResult<
            ApiResponseDto<
                IReadOnlyList<FixedAssetPoLineLookupDto>>>>
        GetFixedAssetPoLines(
            [FromRoute] string poNo,
            [FromQuery] string? excludeAcceptanceNo,
            CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(poNo))
        {
            return BadRequest(
                ApiResponseDto<
                    IReadOnlyList<FixedAssetPoLineLookupDto>>
                    .FromError(
                        "PO Number is required."));
        }

        var result =
            await repository.GetFixedAssetPoLinesAsync(
                poNo.Trim(),
                excludeAcceptanceNo,
                cancellationToken);

        return Ok(
            ApiResponseDto<
                IReadOnlyList<FixedAssetPoLineLookupDto>>
                .FromData(result));
    }

    // ============================================================
    // SUPPLIERS LOOKUP
    // GET: api/asset-acceptances/lookups/suppliers
    // ============================================================

    [HttpGet("lookups/suppliers")]
    public async Task<
        ActionResult<
            ApiResponseDto<
                IReadOnlyList<SupplierLookupDto>>>>
        GetSuppliers(
            [FromQuery] string? search,
            CancellationToken cancellationToken)
    {
        var result =
            await repository.GetSuppliersAsync(
                search,
                cancellationToken);

        return Ok(
            ApiResponseDto<
                IReadOnlyList<SupplierLookupDto>>
                .FromData(result));
    }

    // ============================================================
    // BRANCHES LOOKUP
    // GET: api/asset-acceptances/lookups/branches
    // ============================================================

    [HttpGet("lookups/branches")]
    public async Task<
        ActionResult<
            ApiResponseDto<
                IReadOnlyList<BranchLookupDto>>>>
        GetBranches(
            [FromQuery] string? search,
            CancellationToken cancellationToken)
    {
        var result =
            await repository.GetBranchesAsync(
                search,
                cancellationToken);

        return Ok(
            ApiResponseDto<
                IReadOnlyList<BranchLookupDto>>
                .FromData(result));
    }

    // ============================================================
    // CATEGORIES LOOKUP
    // ============================================================

    [HttpGet("lookups/categories")]
    public Task<
        ActionResult<
            ApiResponseDto<
                IReadOnlyList<CodeNameLookupDto>>>>
        GetCategories(
            [FromQuery] string? search,
            CancellationToken cancellationToken)
    {
        return GetCodeLookup(
            "dbo.usp_AssetAcceptance_GetCategories",
            search,
            cancellationToken);
    }

    // ============================================================
    // DEPRECIATION METHODS LOOKUP
    // ============================================================

    [HttpGet("lookups/depreciation-methods")]
    public Task<
        ActionResult<
            ApiResponseDto<
                IReadOnlyList<CodeNameLookupDto>>>>
        GetDepreciationMethods(
            [FromQuery] string? search,
            CancellationToken cancellationToken)
    {
        return GetCodeLookup(
            "dbo.usp_AssetAcceptance_GetDepreciationMethods",
            search,
            cancellationToken);
    }

    // ============================================================
    // LOCATIONS LOOKUP
    // ============================================================

    [HttpGet("lookups/locations")]
    public Task<
        ActionResult<
            ApiResponseDto<
                IReadOnlyList<CodeNameLookupDto>>>>
        GetLocations(
            [FromQuery] string? search,
            CancellationToken cancellationToken)
    {
        return GetCodeLookup(
            "dbo.usp_AssetAcceptance_GetLocations",
            search,
            cancellationToken);
    }

    // ============================================================
    // CUSTODIANS LOOKUP
    // ============================================================

    [HttpGet("lookups/custodians")]
    public Task<
        ActionResult<
            ApiResponseDto<
                IReadOnlyList<CodeNameLookupDto>>>>
        GetCustodians(
            [FromQuery] string? search,
            CancellationToken cancellationToken)
    {
        return GetCodeLookup(
            "dbo.usp_AssetAcceptance_GetCustodians",
            search,
            cancellationToken);
    }

    // ============================================================
    // GL ACCOUNTS LOOKUP
    // ============================================================

    //[HttpGet("lookups/accounts")]
    //public Task<
    //    ActionResult<
    //        ApiResponseDto<
    //            IReadOnlyList<CodeNameLookupDto>>>>
    //    GetAccounts(
    //        [FromQuery] string? search,
    //        CancellationToken cancellationToken)
    //{
    //    return GetCodeLookup(
    //        "dbo.usp_AssetAcceptance_GetAccounts",
    //        search,
    //        cancellationToken);
    //}

    [HttpGet("lookups/accounts")]
    [ProducesResponseType(
     typeof(
         ApiResponseDto<
             IReadOnlyList<AssetAccountLookupDto>>),
     StatusCodes.Status200OK)]
    [ProducesResponseType(
     typeof(
         ApiResponseDto<
             IReadOnlyList<AssetAccountLookupDto>>),
     StatusCodes.Status400BadRequest)]
    public async Task<
     ActionResult<
         ApiResponseDto<
             IReadOnlyList<AssetAccountLookupDto>>>>
     GetAccounts(
         [FromQuery] AssetAccountLookupQueryDto? query,
         CancellationToken cancellationToken)
    {
        if (query is null)
        {
            return BadRequest(
                ApiResponseDto<
                    IReadOnlyList<AssetAccountLookupDto>>
                    .FromError(
                        "Account lookup request is required."));
        }

        try
        {
            var normalizedPurpose =
                AssetAccountPurposes.Normalize(
                    query.Purpose);

            var result =
                await repository.GetAccountsAsync(
                    normalizedPurpose,
                    query.Search,
                    query.Take,
                    cancellationToken);

            return Ok(
                ApiResponseDto<
                    IReadOnlyList<AssetAccountLookupDto>>
                    .FromData(result));
        }
        catch (ValidationException exception)
        {
            return BadRequest(
                ApiResponseDto<
                    IReadOnlyList<AssetAccountLookupDto>>
                    .FromError(exception.Message));
        }
    }

    // ============================================================
    // PRIVATE WORKFLOW METHOD
    // ============================================================

    private async Task<
        ActionResult<
            ApiResponseDto<AssetAcceptanceCommandResultDto>>>
        ExecuteTransition(
            string documentNo,
            string action,
            WorkflowRequestDto request,
            CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(documentNo))
        {
            return BadRequest(
                ApiResponseDto<AssetAcceptanceCommandResultDto>
                    .FromError(
                        "Asset Acceptance Number is required."));
        }

        if (request is null)
        {
            return BadRequest(
                ApiResponseDto<AssetAcceptanceCommandResultDto>
                    .FromError(
                        "Workflow request is required."));
        }

        var result =
            await repository.TransitionAsync(
                documentNo.Trim(),
                action,
                request,
                cancellationToken);

        return Ok(
            ApiResponseDto<AssetAcceptanceCommandResultDto>
                .FromData(
                    result,
                    result.Message));
    }

    // ============================================================
    // PRIVATE GENERIC CODE LOOKUP METHOD
    // ============================================================

    private async Task<
        ActionResult<
            ApiResponseDto<
                IReadOnlyList<CodeNameLookupDto>>>>
        GetCodeLookup(
            string procedure,
            string? search,
            CancellationToken cancellationToken)
    {
        var result =
            await repository.GetCodeLookupAsync(
                procedure,
                search,
                cancellationToken);

        return Ok(
            ApiResponseDto<
                IReadOnlyList<CodeNameLookupDto>>
                .FromData(result));
    }
}