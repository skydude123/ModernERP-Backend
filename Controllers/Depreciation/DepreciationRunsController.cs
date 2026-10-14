using HRIStwinbeeAPI.DTO.AssetAcceptance;
using HRIStwinbeeAPI.DTO.Depreciation;
using HRIStwinbeeAPI.Services.Depreciation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRIStwinbeeAPI.Controllers.Depreciation;

[ApiController]
[Authorize]
[Route("api/accounting/depreciation-runs")]
public sealed class DepreciationRunsController(IDepreciationService service) : ControllerBase
{
    [HttpGet, Authorize(Policy = "Depreciation.View")]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<DepreciationRunListItemDto>>>> Query(
        [FromQuery] DepreciationRunQueryDto request, CancellationToken token)
        => Ok(ApiResponseDto<PagedResultDto<DepreciationRunListItemDto>>.FromData(
            await service.QueryAsync(request, token)));

    [HttpGet("{depRunNo}"), Authorize(Policy = "Depreciation.View")]
    public async Task<ActionResult<ApiResponseDto<DepreciationRunDto>>> Get(string depRunNo, CancellationToken token)
    {
        var result = await service.GetAsync(depRunNo, token);
        return result is null
            ? NotFound(ApiResponseDto<DepreciationRunDto>.FromError("Depreciation run was not found."))
            : Ok(ApiResponseDto<DepreciationRunDto>.FromData(result));
    }

    [HttpPost("preview"), Authorize(Policy = "Depreciation.Create")]
    public async Task<ActionResult<ApiResponseDto<DepreciationPreviewResponseDto>>> Preview(
        [FromBody] DepreciationPreviewRequestDto request, CancellationToken token)
        => Ok(ApiResponseDto<DepreciationPreviewResponseDto>.FromData(
            await service.PreviewAsync(request, token)));

    [HttpPost, Authorize(Policy = "Depreciation.Create")]
    public async Task<
    ActionResult<
        ApiResponseDto<DepreciationCommandResultDto>>>
    Create(
        [FromBody] DepreciationSaveRequestDto request,
        CancellationToken token)
    {
        if (request is null)
        {
            return BadRequest(
                ApiResponseDto<DepreciationCommandResultDto>
                    .FromError(
                        "Depreciation request is required."));
        }

        /*
         * CREATE must not contain an existing
         * Depreciation Run Number.
         */
        request.DepRunNo = null;

        try
        {
            var result =
                await service.SaveAsync(
                    request,
                    token);

            return CreatedAtAction(
                nameof(Get),
                new
                {
                    depRunNo = result.DepRunNo
                },
                ApiResponseDto<DepreciationCommandResultDto>
                    .FromData(
                        result,
                        result.Message));
        }
        /*
         * Handles custom stored procedure errors
         * raised using THROW or RAISERROR.
         */
        catch (Microsoft.Data.SqlClient.SqlException exception)
            when (exception.Number >= 50000)
        {
            return Conflict(
                ApiResponseDto<DepreciationCommandResultDto>
                    .FromError(
                        exception.Message));
        }
        /*
         * Handles other SQL Server/database errors.
         */
        catch (Microsoft.Data.SqlClient.SqlException exception)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponseDto<DepreciationCommandResultDto>
                    .FromError(
                        $"A database error occurred while creating the Depreciation Run. " +
                        $"SQL Error {exception.Number}: {exception.Message}"));
        }
        /*
         * Handles a cancelled HTTP request.
         */
        catch (OperationCanceledException)
            when (token.IsCancellationRequested)
        {
            return StatusCode(
                StatusCodes.Status499ClientClosedRequest,
                ApiResponseDto<DepreciationCommandResultDto>
                    .FromError(
                        "The Depreciation Run create request was cancelled."));
        }
        /*
         * Handles all other unexpected errors.
         */
        catch (Exception exception)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponseDto<DepreciationCommandResultDto>
                    .FromError(
                        $"An unexpected error occurred while creating the Depreciation Run: " +
                        exception.Message));
        }
    }

    [HttpPut("{depRunNo}"), Authorize(Policy = "Depreciation.EditDraft")]
    public async Task<ActionResult<ApiResponseDto<DepreciationCommandResultDto>>> Update(
        string depRunNo, [FromBody] DepreciationSaveRequestDto request, CancellationToken token)
    {
        request.DepRunNo = depRunNo;
        var result = await service.SaveAsync(request, token);
        return Ok(ApiResponseDto<DepreciationCommandResultDto>.FromData(result, result.Message));
    }

    [HttpDelete("{depRunNo}"), Authorize(Policy = "Depreciation.DeleteDraft")]
    public Task<ActionResult<ApiResponseDto<DepreciationCommandResultDto>>> Delete(
        string depRunNo, [FromBody] DepreciationCommandRequestDto request, CancellationToken token)
        => Transition(depRunNo, "delete", request, token);

    [HttpPost("{depRunNo}/submit"), Authorize(Policy = "Depreciation.Submit")]
    public Task<ActionResult<ApiResponseDto<DepreciationCommandResultDto>>> Submit(
        string depRunNo, [FromBody] DepreciationCommandRequestDto request, CancellationToken token)
        => Transition(depRunNo, "submit", request, token);

    [HttpPost("{depRunNo}/approve"), Authorize(Policy = "Depreciation.Approve")]
    public Task<ActionResult<ApiResponseDto<DepreciationCommandResultDto>>> Approve(
        string depRunNo, [FromBody] DepreciationCommandRequestDto request, CancellationToken token)
        => Transition(depRunNo, "approve", request, token);

    [HttpPost("{depRunNo}/post"), Authorize(Policy = "Depreciation.Post")]
    public Task<ActionResult<ApiResponseDto<DepreciationCommandResultDto>>> Post(
        string depRunNo, [FromBody] DepreciationCommandRequestDto request, CancellationToken token)
        => Transition(depRunNo, "post", request, token);

    [HttpPost("{depRunNo}/cancel"), Authorize(Policy = "Depreciation.EditDraft")]
    public Task<ActionResult<ApiResponseDto<DepreciationCommandResultDto>>> Cancel(
        string depRunNo, [FromBody] DepreciationCommandRequestDto request, CancellationToken token)
        => Transition(depRunNo, "cancel", request, token);

    [HttpPost("{depRunNo}/reverse"), Authorize(Policy = "Depreciation.Reverse")]
    public async Task<ActionResult<ApiResponseDto<DepreciationCommandResultDto>>> Reverse(
        string depRunNo, [FromBody] DepreciationReverseRequestDto request, CancellationToken token)
    {
        var result = await service.ReverseAsync(depRunNo, request, token);
        return Ok(ApiResponseDto<DepreciationCommandResultDto>.FromData(result, result.Message));
    }

    [HttpGet("eligible-assets"), Authorize(Policy = "Depreciation.View")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<DepreciationPreviewLineDto>>>> EligibleAssets(
        [FromQuery] DepreciationPreviewRequestDto request, CancellationToken token)
        => Ok(ApiResponseDto<IReadOnlyList<DepreciationPreviewLineDto>>.FromData(
            await service.GetEligibleAssetsAsync(request, token)));

    [HttpGet("assets/{assetId:long}/history"), Authorize(Policy = "Depreciation.View")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<DepreciationAssetHistoryDto>>>> AssetHistory(
        long assetId, CancellationToken token)
        => Ok(ApiResponseDto<IReadOnlyList<DepreciationAssetHistoryDto>>.FromData(
            await service.GetAssetHistoryAsync(assetId, token)));

    [HttpGet("lookups/branches"), Authorize(Policy = "Depreciation.View")]
    public Task<ActionResult<ApiResponseDto<IReadOnlyList<DepreciationLookupDto>>>> Branches(
        [FromQuery] string? search, CancellationToken token)
        => Lookup("dbo.usp_Depreciation_GetBranches", search, token);

    [HttpGet("lookups/categories"), Authorize(Policy = "Depreciation.View")]
    public Task<ActionResult<ApiResponseDto<IReadOnlyList<DepreciationLookupDto>>>> Categories(
        [FromQuery] string? search, CancellationToken token)
        => Lookup("dbo.usp_Depreciation_GetAssetCategories", search, token);

    [HttpGet("lookups/methods"), Authorize(Policy = "Depreciation.View")]
    public Task<ActionResult<ApiResponseDto<IReadOnlyList<DepreciationLookupDto>>>> Methods(
        [FromQuery] string? search, CancellationToken token)
        => Lookup("dbo.usp_Depreciation_GetMethods", search, token);

    [HttpGet("lookups/conventions"), Authorize(Policy = "Depreciation.View")]
    public Task<ActionResult<ApiResponseDto<IReadOnlyList<DepreciationLookupDto>>>> Conventions(
        [FromQuery] string? search, CancellationToken token)
        => Lookup("dbo.usp_Depreciation_GetConventions", search, token);

    [HttpGet("lookups/posting-accounts"), Authorize(Policy = "Depreciation.View")]
    public Task<ActionResult<ApiResponseDto<IReadOnlyList<DepreciationLookupDto>>>> PostingAccounts(
        [FromQuery] string? search, CancellationToken token)
        => Lookup("dbo.usp_Depreciation_GetPostingAccounts", search, token);

    private async Task<ActionResult<ApiResponseDto<DepreciationCommandResultDto>>> Transition(
        string depRunNo, string action, DepreciationCommandRequestDto request, CancellationToken token)
    {
        var result = await service.TransitionAsync(depRunNo, action, request, token);
        return Ok(ApiResponseDto<DepreciationCommandResultDto>.FromData(result, result.Message));
    }

    private async Task<ActionResult<ApiResponseDto<IReadOnlyList<DepreciationLookupDto>>>> Lookup(
        string procedure, string? search, CancellationToken token)
        => Ok(ApiResponseDto<IReadOnlyList<DepreciationLookupDto>>.FromData(
            await service.GetLookupAsync(procedure, search, token)));
}
