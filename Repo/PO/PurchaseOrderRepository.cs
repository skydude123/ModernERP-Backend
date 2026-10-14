using Dapper;
using HRIStwinbeeAPI.DTO.PO;
using HRIStwinbeeAPI.Repo.Interface.PO;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace HRIStwinbeeAPI.Repo.PO
{
    public class PurchaseOrderRepository : IPurchaseOrderRepository
    {
        private readonly IConfiguration _configuration;

        public PurchaseOrderRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection CreateConnection()
        {
            var connectionString = _configuration.GetConnectionString("INVENTORY");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("DefaultConnection is missing in appsettings.json.");

            return new SqlConnection(connectionString);
        }

        public async Task<IReadOnlyList<SupplierLookupDto>> GetSuppliersAsync()
        {
            await using var connection = CreateConnection();

            var result = await connection.QueryAsync<SupplierLookupDto>(
                "dbo.usp_PurchaseOrder_LookupSuppliers",
                commandType: CommandType.StoredProcedure);

            return result.AsList();
        }

        public async Task<IReadOnlyList<BranchLookupDto>> GetBranchesAsync()
        {
            await using var connection = CreateConnection();

            var result = await connection.QueryAsync<BranchLookupDto>(
                "dbo.usp_PurchaseOrder_LookupBranches",
                commandType: CommandType.StoredProcedure);

            return result.AsList();
        }

        public async Task<IReadOnlyList<TermLookupDto>> GetTermsAsync()
        {
            await using var connection = CreateConnection();

            var result = await connection.QueryAsync<TermLookupDto>(
                "dbo.usp_PurchaseOrder_LookupTerms",
                commandType: CommandType.StoredProcedure);

            return result.AsList();
        }

        public async Task<IReadOnlyList<VatTypeLookupDto>> GetVatTypesAsync()
        {
            await using var connection = CreateConnection();

            var result = await connection.QueryAsync<VatTypeLookupDto>(
                "dbo.usp_PurchaseOrder_LookupVatTypes",
                commandType: CommandType.StoredProcedure);

            return result.AsList();
        }

        public async Task<IReadOnlyList<SoVerificationDto>> VerifySoNoAsync(string soNo)
        {
            await using var connection = CreateConnection();

            var p = new DynamicParameters();
            p.Add("@so_no", soNo, DbType.String, size: 10);

            var result = await connection.QueryAsync<SoVerificationDto>(
                "dbo.usp_PurchaseOrder_VerifySONo",
                p,
                commandType: CommandType.StoredProcedure);

            return result.AsList();
        }

        public async Task<IReadOnlyList<ItemLookupDto>> GetItemsAsync(string? search)
        {
            await using var connection = CreateConnection();

            var p = new DynamicParameters();
            p.Add("@search", search, DbType.String, size: 100);

            var result = await connection.QueryAsync<ItemLookupDto>(
                "dbo.usp_PurchaseOrder_LookupItems",
                p,
                commandType: CommandType.StoredProcedure);

            return result.AsList();
        }

        public async Task<PurchaseOrderGetResponseDto> GetByNoAsync(string poNo)
        {
            await using var connection = CreateConnection();

            var p = new DynamicParameters();
            p.Add("@po_no", poNo, DbType.String, size: 15);

            using var grid = await connection.QueryMultipleAsync(
                "dbo.usp_PurchaseOrder_GetByNo",
                p,
                commandType: CommandType.StoredProcedure);

            var header = await grid.ReadFirstOrDefaultAsync<PurchaseOrderHeaderDto>();
            var details = (await grid.ReadAsync<PurchaseOrderDetailResultDto>()).AsList();

            return new PurchaseOrderGetResponseDto
            {
                Header = header,
                Details = details
            };
        }

        public async Task<PurchaseOrderSaveResponseDto> SaveAsync(
            PurchaseOrderSaveRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (request.Details.Count == 0)
                throw new ArgumentException("Purchase Order must have at least one detail line.");

            await using var connection = CreateConnection();

            await connection.OpenAsync(cancellationToken);
            await using var transaction =
                (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

            var detailsTable = BuildDetailDataTable(request.Details);
            var pricingDetailsTable = BuildPricingDetailDataTable(request.Details);

            var p = new DynamicParameters();

            p.Add("@po_no", request.PoNo, DbType.String, ParameterDirection.InputOutput, size: 15);
            p.Add("@po_date", request.PoDate.Date, DbType.Date);
            p.Add("@po_del_date", request.PoDelDate?.Date, DbType.Date);
            p.Add("@po_type", request.PoType, DbType.String, size: 4);
            p.Add("@term_code", request.TermCode, DbType.String, size: 4);
            p.Add("@status", request.Status, DbType.String, size: 1);
            p.Add("@vat_type", request.VatType, DbType.String, size: 1);
            p.Add("@cheque_type", request.ChequeType, DbType.String, size: 1);
            p.Add("@suppcode", request.SuppCode, DbType.String, size: 10);
            p.Add("@branchname", request.BranchName, DbType.String, size: 15);
            p.Add("@so_no", request.SoNo, DbType.String, size: 10);
            p.Add("@remarks", request.Remarks, DbType.String, size: 255);
            p.Add("@custid", request.CustId, DbType.Int32);
            p.Add("@user_id", request.UserId, DbType.String, size: 50);
            p.Add("@Details", detailsTable.AsTableValuedParameter("dbo.PurchaseOrderDetailUpsertType"));
            p.Add("@ReplaceDetails", request.ReplaceDetails, DbType.Boolean);
            p.Add("@ReturnPONumber", dbType: DbType.String, direction: ParameterDirection.Output, size: 15);

            try
            {
                PurchaseOrderSaveHeaderResultDto? headerResult;

                var saveCommand = new CommandDefinition(
                    "dbo.usp_PurchaseOrder_SaveUpsert",
                    p,
                    transaction,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken);

                using (var grid = await connection.QueryMultipleAsync(saveCommand))
                {
                    headerResult = await grid.ReadFirstOrDefaultAsync<PurchaseOrderSaveHeaderResultDto>();
                    _ = (await grid.ReadAsync<PurchaseOrderDetailResultDto>()).AsList();
                }

                var returnPoNumber = p.Get<string>("@ReturnPONumber");
                var poNo = p.Get<string>("@po_no")
                    ?? headerResult?.PoNo
                    ?? returnPoNumber
                    ?? string.Empty;

                if (string.IsNullOrWhiteSpace(poNo))
                {
                    throw new InvalidOperationException(
                        "The Purchase Order save did not return a PO number.");
                }

                var pricingParameters = new DynamicParameters();
                pricingParameters.Add("@po_no", poNo, DbType.String, size: 15);
                pricingParameters.Add(
                    "@details",
                    pricingDetailsTable.AsTableValuedParameter(
                        "dbo.PurchaseOrderPricingDetailType"));

                var pricingCommand = new CommandDefinition(
                    "dbo.usp_PurchasePrice_ValidateAndApplyPoConversions",
                    pricingParameters,
                    transaction,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken);

                var details = (await connection.QueryAsync<PurchaseOrderDetailResultDto>(
                    pricingCommand)).AsList();

                await transaction.CommitAsync(cancellationToken);

                return new PurchaseOrderSaveResponseDto
                {
                    PoNo = poNo,
                    ReturnPONumber = returnPoNumber ?? headerResult?.ReturnPONumber ?? poNo,
                    ResultMessage = headerResult?.ResultMessage ?? "SUCCESS",
                    Details = details
                };
            }
            catch
            {
                try
                {
                    if (transaction.Connection is not null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                    }
                }
                catch (InvalidOperationException)
                {
                    // The legacy save procedure can roll back the outer transaction.
                }

                throw;
            }
        }

        private static DataTable BuildDetailDataTable(IEnumerable<PurchaseOrderDetailSaveDto> details)
        {
            var table = new DataTable();

            table.Columns.Add("row_no", typeof(int));
            table.Columns.Add("po_line_no", typeof(int));
            table.Columns.Add("item_code", typeof(string));
            table.Columns.Add("qty_po", typeof(decimal));
            table.Columns.Add("po_um", typeof(string));
            table.Columns.Add("po_conv_factor", typeof(decimal));
            table.Columns.Add("unit_price", typeof(decimal));

            foreach (var detail in details)
            {
                table.Rows.Add(
                    detail.RowNo,
                    detail.PoLineNo.HasValue && detail.PoLineNo.Value > 0 ? detail.PoLineNo.Value : DBNull.Value,
                    detail.ItemCode?.Trim() ?? string.Empty,
                    detail.QtyPo,
                    detail.PoUm?.Trim() ?? string.Empty,
                    detail.PoConvFactor <= 0 ? 1 : detail.PoConvFactor,
                    detail.UnitPrice
                );
            }

            return table;

        }

        private static DataTable BuildPricingDetailDataTable(
            IEnumerable<PurchaseOrderDetailSaveDto> details)
        {
            var table = new DataTable();

            table.Columns.Add("row_no", typeof(int));
            table.Columns.Add("po_line_no", typeof(int));
            table.Columns.Add("item_code", typeof(string));
            table.Columns.Add("po_um", typeof(string));
            table.Columns.Add("po_conv_factor", typeof(decimal));

            foreach (var detail in details)
            {
                table.Rows.Add(
                    detail.RowNo,
                    detail.PoLineNo.HasValue && detail.PoLineNo.Value > 0
                        ? detail.PoLineNo.Value
                        : DBNull.Value,
                    detail.ItemCode.Trim(),
                    detail.PoUm.Trim(),
                    detail.PoConvFactor);
            }

            return table;
        }

        public async Task<PagedResultDto<PurchaseOrderListItemDto>> GetListPageAsync(PurchaseOrderListQueryDto query)
        {
            await using var connection = CreateConnection();

            var p = new DynamicParameters();

            p.Add("@from_date", query.FromDate?.Date, DbType.Date);
            p.Add("@to_date", query.ToDate?.Date, DbType.Date);
            p.Add("@status", string.IsNullOrWhiteSpace(query.Status) ? "ALL" : query.Status.Trim(), DbType.String, size: 10);
            p.Add("@branchname", string.IsNullOrWhiteSpace(query.BranchName) ? null : query.BranchName.Trim(), DbType.String, size: 15);
            p.Add("@search", string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim(), DbType.String, size: 100);
            p.Add("@skip", query.Skip < 0 ? 0 : query.Skip, DbType.Int32);
            p.Add("@take", query.Take <= 0 ? 50 : query.Take, DbType.Int32);
            p.Add("@sort_field", string.IsNullOrWhiteSpace(query.SortField) ? "poDate" : query.SortField.Trim(), DbType.String, size: 50);
            p.Add("@sort_dir", string.IsNullOrWhiteSpace(query.SortDir) ? "desc" : query.SortDir.Trim(), DbType.String, size: 4);
            //p.Add("userId", string.IsNullOrWhiteSpace(query.UserId));
            p.Add(
    "userId",
    query.UserId.Trim().ToUpperInvariant(),
    DbType.AnsiString,
    size: 10);

            var rows = (await connection.QueryAsync<PurchaseOrderListItemDto>(
                "dbo.usp_PurchaseOrder_ListPage",
                p,
                commandType: CommandType.StoredProcedure)).AsList();

            var total = rows.Count == 0 ? 0 : rows[0].TotalCount;

            return new PagedResultDto<PurchaseOrderListItemDto>
            {
                Data = rows,
                Total = total
            };
        }

        public async Task<PurchaseOrderStatusActionResultDto> StatusActionAsync(PurchaseOrderStatusActionRequestDto request)
        {
            await using var connection = CreateConnection();

            var p = new DynamicParameters();

            p.Add("@po_no", request.PoNo, DbType.String, size: 15);
            p.Add("@action", request.Action, DbType.String, size: 20);
            p.Add("@user_id", request.UserId, DbType.String, size: 50);
            p.Add("@remarks", request.Remarks, DbType.String, size: 255);

            var result = await connection.QueryFirstAsync<PurchaseOrderStatusActionResultDto>(
                "dbo.usp_PurchaseOrder_StatusAction",
                p,
                commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<IReadOnlyList<UmDto>> GetUmAsync()
        {
            await using var connection = CreateConnection();

            var result = await connection.QueryAsync<UmDto>(
                "dbo.lookupUM",
                commandType: CommandType.StoredProcedure);

            return result.AsList();
        }

        public async Task<PurchaseOrderStatusActionResultDto> ReverseApprovalAsync(
    PurchaseOrderReverseApprovalRequestDto request,
    CancellationToken cancellationToken = default)
        {
            await using var connection = CreateConnection();

            var p = new DynamicParameters();

            p.Add("@po_no", request.PoNo, DbType.String, size: 15);
            p.Add("@user_id", request.UserId, DbType.String, size: 50);
            p.Add("@remarks", request.Remarks, DbType.String, size: 255);

            var command = new CommandDefinition(
                commandText: "dbo.usp_PurchaseOrder_ReverseApproval",
                parameters: p,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken
            );

            var result = await connection.QuerySingleAsync<PurchaseOrderStatusActionResultDto>(command);

            return result;
        }
    }
}
