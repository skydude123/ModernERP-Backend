using HRIStwinbeeAPI.DTO.Inventory.ItemWarehouse;
using HRIStwinbeeAPI.DTO.Suppliers;
using HRIStwinbeeAPI.Services.Suppliers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRIStwinbeeAPI.Controllers.Suppliers;

[ApiController]
//[Authorize]
[Route("api/master-data/suppliers")]
[TypeFilter(typeof(SupplierExceptionFilter))]
public sealed class SuppliersController(ISupplierService service)
    : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = SupplierAuthorizationPolicies.View)]
    public async Task<
        ActionResult<
            ApiResponseDto<
                SupplierPagedResult<SupplierListItemDto>>>> Query(
        [FromQuery] SupplierListQueryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await service.QueryAsync(request, cancellationToken);

        return Ok(Success(
            result,
            "Supplier records loaded successfully."));
    }

    [HttpGet("{supplierCode}")]
    [Authorize(Policy = SupplierAuthorizationPolicies.View)]
    public async Task<
        ActionResult<ApiResponseDto<SupplierDetailDto>>> GetByCode(
        [FromRoute] string supplierCode,
        CancellationToken cancellationToken)
    {
        var result = await service.GetByCodeAsync(
            supplierCode,
            cancellationToken);

        if (result is null)
        {
            throw new SupplierNotFoundException(
                $"Supplier '{supplierCode.Trim()}' was not found.");
        }

        return Ok(Success(
            result,
            "Supplier loaded successfully."));
    }

    [HttpGet("lookups/groups")]
    [Authorize(Policy = SupplierAuthorizationPolicies.View)]
    public async Task<
        ActionResult<
            ApiResponseDto<IReadOnlyList<SupplierLookupDto>>>> GetGroups(
        [FromQuery] string? search,
        CancellationToken cancellationToken) =>
        Ok(Success(
            await service.GetGroupsAsync(search, cancellationToken),
            "Supplier groups loaded successfully."));

    [HttpGet("lookups/terms")]
    [Authorize(Policy = SupplierAuthorizationPolicies.View)]
    public async Task<
        ActionResult<
            ApiResponseDto<IReadOnlyList<SupplierLookupDto>>>> GetTerms(
        [FromQuery] string? search,
        CancellationToken cancellationToken) =>
        Ok(Success(
            await service.GetTermsAsync(search, cancellationToken),
            "Payment terms loaded successfully."));

    [HttpGet("lookups/currencies")]
    [Authorize(Policy = SupplierAuthorizationPolicies.View)]
    public async Task<
        ActionResult<
            ApiResponseDto<IReadOnlyList<SupplierLookupDto>>>> GetCurrencies(
        [FromQuery] string? search,
        CancellationToken cancellationToken) =>
        Ok(Success(
            await service.GetCurrenciesAsync(search, cancellationToken),
            "Currencies loaded successfully."));

    [HttpGet("lookups/tax-profiles")]
    [Authorize(Policy = SupplierAuthorizationPolicies.View)]
    public async Task<
        ActionResult<
            ApiResponseDto<IReadOnlyList<SupplierLookupDto>>>> GetTaxProfiles(
        [FromQuery] string? search,
        CancellationToken cancellationToken) =>
        Ok(Success(
            await service.GetTaxProfilesAsync(search, cancellationToken),
            "Tax profiles loaded successfully."));

    [HttpPost]
    [Authorize(Policy = SupplierAuthorizationPolicies.Create)]
    public async Task<ActionResult<ApiResponseDto<SupplierCommandResultDto>>> Create([FromBody] SupplierCreateRequest request,CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        var response = Success(result, result.Message);

        return CreatedAtAction(
            nameof(GetByCode),
            new { supplierCode = result.Supplier.SupplierCode },
            response);
    }

    [HttpPut("{supplierCode}")]
    [Authorize(Policy = SupplierAuthorizationPolicies.Edit)]
    public async Task<ActionResult<ApiResponseDto<SupplierCommandResultDto>>> Update([FromRoute] string supplierCode, [FromBody] SupplierUpdateRequest request,CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(
            supplierCode,
            request,
            cancellationToken);

        return Ok(Success(result, result.Message));
    }

    [HttpPost("{supplierCode}/activate")]
    [Authorize(Policy = SupplierAuthorizationPolicies.Activate)]
    public Task<
        ActionResult<ApiResponseDto<SupplierCommandResultDto>>> Activate(
        [FromRoute] string supplierCode,
        //[FromBody] SupplierStatusRequest request,
        CancellationToken cancellationToken) =>
        ChangeStatus(
            supplierCode,
            "Y",
            //request,
            cancellationToken);

    [HttpPost("{supplierCode}/deactivate")]
    [Authorize(Policy = SupplierAuthorizationPolicies.Deactivate)]
    public Task<
        ActionResult<ApiResponseDto<SupplierCommandResultDto>>> Deactivate(
        [FromRoute] string supplierCode,
        //[FromBody] SupplierStatusRequest request,
        CancellationToken cancellationToken) =>
        ChangeStatus(
            supplierCode,
            "N",
            //request,
            cancellationToken);

    private async Task<
        ActionResult<ApiResponseDto<SupplierCommandResultDto>>> ChangeStatus(
        string supplierCode,
        string activeFlag,
        //SupplierStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await service.SetActiveStatusAsync(
            supplierCode,
            activeFlag,
            //request,
            cancellationToken);

        return Ok(Success(result, result.Message));
    }

    private static ApiResponseDto<T> Success<T>(
        T data,
        string message) =>
        new()
        {
            Success = true,
            Message = message,
            Data = data,
            Error = null
        };
}
