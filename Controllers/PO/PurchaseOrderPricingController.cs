using HRIStwinbeeAPI.Models.PO;
using HRIStwinbeeAPI.Services.PO;
using HRIStwinbeeAPI.Services.Suppliers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRIStwinbeeAPI.Controllers.PO;

[ApiController]
[Authorize]
[Route("api/purchase-orders/pricing")]
[TypeFilter(typeof(PurchasePriceExceptionFilter))]
public sealed class PurchaseOrderPricingController(
    IPurchasePriceService service) : ControllerBase
{
    [HttpGet("supplier-price-list")]
    public async Task<ActionResult<IReadOnlyList<SupplierPriceListItem>>> GetSupplierPriceList(
    CancellationToken cancellationToken) =>
    Ok(await service.GetSupplierPriceListAsync(
        cancellationToken));

    //[HttpGet("supplier-price-list")]
    //public async Task<ActionResult<IReadOnlyList<SupplierPriceListItem>>> GetSupplierPriceList(
    // [FromQuery] string supplierCode,
    // CancellationToken cancellationToken) =>
    // Ok(await service.GetSupplierPriceListAsync(
    //     supplierCode,
    //     cancellationToken));

    [HttpGet("current")]
    public async Task<ActionResult<PurchasePricePagedResult<SupplierPriceRecord>>> GetCurrent(
        [FromQuery] PurchasePriceCurrentQuery query,
        CancellationToken cancellationToken) =>
        Ok(await service.GetCurrentPageAsync(query, cancellationToken));

    [HttpPut("current")]
    [Authorize(Policy = SupplierAuthorizationPolicies.Edit)]
    public async Task<ActionResult<SupplierPriceUpsertResult>> UpsertCurrent(
        [FromBody] SupplierPriceUpsertRequest request,
        CancellationToken cancellationToken) =>
        Ok(await service.UpsertAsync(request, cancellationToken));

    [HttpGet("item-uoms/{itemCode}")]
    public async Task<ActionResult<IReadOnlyList<PurchasePriceItemUm>>> GetItemUms(
        [FromRoute] string itemCode,
        CancellationToken cancellationToken) =>
        Ok(await service.GetItemUmsAsync(itemCode, cancellationToken));

    [HttpGet("resolve")]
    public async Task<ActionResult<PurchasePriceResolution>> Resolve(
        [FromQuery] string supplierCode,
        [FromQuery] string itemCode,
        [FromQuery] string poUm,
        [FromQuery] DateTime? asOfDate,
        CancellationToken cancellationToken) =>
        Ok(await service.ResolveAsync(
            supplierCode,
            itemCode,
            poUm,
            asOfDate,
            cancellationToken));

    [HttpGet("inquiry")]
    public async Task<ActionResult<PurchasePriceInquiryResult>> Inquiry(
        [FromQuery] string supplierCode,
        [FromQuery] string itemCode,
        [FromQuery] string poUm,
        [FromQuery] DateTime? asOfDate,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default) =>
        Ok(await service.GetInquiryAsync(
            supplierCode,
            itemCode,
            poUm,
            asOfDate,
            skip,
            take,
            cancellationToken));

    [HttpGet("history/master")]
    public async Task<ActionResult<PurchasePricePagedResult<SupplierPriceHistoryRecord>>> MasterHistory(
        [FromQuery] PurchasePriceHistoryQuery query,
        CancellationToken cancellationToken) =>
        Ok(await service.GetMasterHistoryPageAsync(query, cancellationToken));

    [HttpGet("history/actual-po")]
    public async Task<ActionResult<PurchasePricePagedResult<ActualPoPriceHistoryRecord>>> ActualPoHistory(
        [FromQuery] PurchasePriceHistoryQuery query,
        CancellationToken cancellationToken) =>
        Ok(await service.GetActualPoHistoryPageAsync(query, cancellationToken));
}
