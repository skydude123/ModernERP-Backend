using HRIStwinbeeAPI.DTO.PO;
using HRIStwinbeeAPI.Repo.Interface.PO;
using Microsoft.AspNetCore.Mvc;

namespace HRIStwinbeeAPI.Controllers.PO;

[Route("api/purchase-order-reports")]
[ApiController]
public sealed class PurchaseOrderReportsController(IPurchaseOrderReportRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PurchaseOrderReportPageDto>> Query([FromQuery] PurchaseOrderReportQueryDto request, CancellationToken cancellationToken)
    {
        if (request.PoDateFrom > request.PoDateTo || request.DeliveryDateFrom > request.DeliveryDateTo)
            return BadRequest(new { message = "The From date cannot be after the To date." });
        if (!new[] { "PO_SUMMARY", "PO_TRACKING", "PO_BALANCE_SUPPLIER", "PO_SUMMARY_ITEM", "PO_RR", "PO_BALANCE" }.Contains(request.ReportType, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { message = "Unsupported purchase order report type." });
        if (!new[] { "ALL", "SELECTED", "NONE" }.Contains(request.SupplierMode, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { message = "Invalid supplier selection mode." });
        if (string.Equals(request.SupplierMode, "SELECTED", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(request.SupplierCodes))
            return BadRequest(new { message = "Select at least one supplier or use All suppliers." });
        return Ok(await repository.QueryAsync(request, cancellationToken));
    }
}
