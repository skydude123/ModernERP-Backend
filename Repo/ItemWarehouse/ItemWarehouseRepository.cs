using Dapper;
using HRIStwinbeeAPI.DTO.Inventory.ItemWarehouse;
using HRIStwinbeeAPI.Repo.Interface.ItemWarehouse;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HRIStwinbeeAPI.Repo.ItemWarehouse
{
    public class ItemWarehouseRepository : IItemWarehouseRepository
    {
        private readonly IConfiguration _configuration;

        public ItemWarehouseRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(
                _configuration.GetConnectionString("INVENTORY")
            );
        }

        public async Task<IEnumerable<ItemWarehouseBalanceDto>> GetByItemCodeAsync(
            string itemCode,
            string active = "Y"
        )
        {
            using var connection = CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@item_code", itemCode);
            parameters.Add("@active", string.IsNullOrWhiteSpace(active) ? "Y" : active);

            var result = await connection.QueryAsync<ItemWarehouseBalanceDto>(
                "[sp].[item_whse_get_by_item_code]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }

    }
}
