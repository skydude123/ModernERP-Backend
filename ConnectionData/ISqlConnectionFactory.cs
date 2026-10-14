using Microsoft.Data.SqlClient;

namespace HRIStwinbeeAPI.ConnectionData
{
    public interface ISqlConnectionFactory
    {
        Task<SqlConnection> CreateOpenConnectionAsync(
  string connectionName = DbConnectionNames.Default,
  CancellationToken cancellationToken = default);
    }
}
