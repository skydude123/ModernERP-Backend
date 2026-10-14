using HRIStwinbeeAPI.DTO.AssetAcceptance;
using HRIStwinbeeAPI.DTO.StockItemInquiry;
using HRIStwinbeeAPI.Services.StockItemInquiry;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRIStwinbeeAPI.Controllers.StockItemInquiry;

[ApiController]
[Authorize(Policy = "StockItemInquiry.View")]
[Route("api/inventory/stock-item-inquiry")]
public sealed class StockItemInquiryController(
    IStockItemInquiryService service,
    ILogger<StockItemInquiryController> logger) : ControllerBase
{
    [HttpGet("items")]
    public Task<ActionResult<ApiResponseDto<StockItemPagedResultDto<StockItemDto>>>> Items(
        [FromQuery] StockItemListQueryDto query,
        CancellationToken token) =>
        ExecuteAsync(() => service.GetItemsAsync(query, token), "load stock items", token);

    [HttpGet("items/{itemCode}/warehouses")]
    public Task<ActionResult<ApiResponseDto<IReadOnlyList<WarehouseStockDto>>>> Warehouses(
        string itemCode,
        CancellationToken token) =>
        ExecuteAsync(() => service.GetWarehouseStocksAsync(itemCode, token), "load warehouse stocks", token);

    [HttpGet("items/{itemCode}/warehouses/{whseCode}/lots")]
    public Task<ActionResult<ApiResponseDto<IReadOnlyList<WarehouseLotDto>>>> Lots(
        string itemCode,
        string whseCode,
        [FromQuery] bool openOnly = true,
        CancellationToken token = default) =>
        ExecuteAsync(() => service.GetWarehouseLotsAsync(itemCode, whseCode, openOnly, token), "load warehouse lots", token);

    [HttpGet("items/{itemCode}/warehouses/{whseCode}/stock-card")]
    public Task<ActionResult<ApiResponseDto<IReadOnlyList<StockCardDto>>>> StockCard(
        string itemCode,
        string whseCode,
        [FromQuery] StockCardQueryDto query,
        CancellationToken token) =>
        ExecuteAsync(() => service.GetStockCardAsync(itemCode, whseCode, query, token), "load stock card", token);

    private async Task<ActionResult<ApiResponseDto<T>>> ExecuteAsync<T>(
        Func<Task<T>> action,
        string operation,
        CancellationToken token)
    {
        try
        {
            return Ok(ApiResponseDto<T>.FromData(await action()));
        }
        catch (ArgumentException exception)
        {
            logger.LogWarning(exception, "Invalid request while attempting to {Operation}.", operation);
            return BadRequest(ApiResponseDto<T>.FromError(exception.Message));
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected error while attempting to {Operation}.", operation);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponseDto<T>.FromError("The inventory inquiry could not be completed."));
        }
    }
}
