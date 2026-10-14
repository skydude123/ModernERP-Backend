using System.Data;
using Dapper;
using HRIStwinbeeAPI.DTO.PO;
using HRIStwinbeeAPI.Repo.Interface.PO;
using Microsoft.Data.SqlClient;

namespace HRIStwinbeeAPI.Repo.PO;

public sealed class PurchaseOrderReportRepository(IConfiguration configuration) : IPurchaseOrderReportRepository
{
    public async Task<PurchaseOrderReportPageDto> QueryAsync(PurchaseOrderReportQueryDto request, CancellationToken cancellationToken)
    {
        var connectionString = configuration.GetConnectionString("INVENTORY")
            ?? throw new InvalidOperationException("INVENTORY connection string is missing.");
        await using var connection = new SqlConnection(connectionString);
        var parameters = new DynamicParameters();
        parameters.Add("@report_type", Normalize(request.ReportType), DbType.AnsiString, size: 25);
        parameters.Add("@po_date_from", request.PoDateFrom?.Date, DbType.Date);
        parameters.Add("@po_date_to", request.PoDateTo?.Date, DbType.Date);
        parameters.Add("@delivery_date_from", request.DeliveryDateFrom?.Date, DbType.Date);
        parameters.Add("@delivery_date_to", request.DeliveryDateTo?.Date, DbType.Date);
        parameters.Add("@po_status", Normalize(request.PoStatus), DbType.AnsiString, size: 10);
        parameters.Add("@balance_status", Normalize(request.BalanceStatus), DbType.AnsiString, size: 20);
        parameters.Add("@po_type", Normalize(request.PoType), DbType.AnsiString, size: 10);
        parameters.Add("@item_code", Normalize(request.ItemCode), DbType.AnsiString, size: 20);
        parameters.Add("@item_desc", Normalize(request.ItemDesc), DbType.AnsiString, size: 100);
        parameters.Add("@po_no", Normalize(request.PoNo), DbType.AnsiString, size: 15);
        parameters.Add("@purchaser", Normalize(request.Purchaser), DbType.AnsiString, size: 50);
        parameters.Add("@branch_name", Normalize(request.BranchName), DbType.AnsiString, size: 50);
        parameters.Add("@supplier_mode", Normalize(request.SupplierMode), DbType.AnsiString, size: 10);
        parameters.Add("@supplier_codes", Normalize(request.SupplierCodes), DbType.AnsiString, size: 4000);
        parameters.Add("@skip", Math.Max(0, request.Skip), DbType.Int32);
        parameters.Add("@take", Math.Clamp(request.Take, 1, 500), DbType.Int32);
        var command = new CommandDefinition("dbo.usp_PO_Report_Query", parameters, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
        using var results = await connection.QueryMultipleAsync(command);
        return new PurchaseOrderReportPageDto { Data = (await results.ReadAsync<PurchaseOrderReportRowDto>()).AsList(), Total = await results.ReadSingleAsync<long>() };
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
