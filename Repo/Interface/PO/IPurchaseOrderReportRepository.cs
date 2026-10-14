using HRIStwinbeeAPI.DTO.PO;

namespace HRIStwinbeeAPI.Repo.Interface.PO;

public interface IPurchaseOrderReportRepository
{
    Task<PurchaseOrderReportPageDto> QueryAsync(PurchaseOrderReportQueryDto request, CancellationToken cancellationToken);
}
