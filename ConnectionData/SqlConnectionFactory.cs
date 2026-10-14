using Microsoft.Data.SqlClient;

namespace HRIStwinbeeAPI.ConnectionData
{
    public sealed class SqlConnectionFactory(IConfiguration configuration) : ISqlConnectionFactory
    {
        public async Task<SqlConnection> CreateOpenConnectionAsync(
            string connectionName = DbConnectionNames.Default,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionName))
            {
                throw new ArgumentException("Connection name is required.", nameof(connectionName));
            }

            var connectionString = configuration.GetConnectionString(connectionName);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    $"Connection string '{connectionName}' is missing in appsettings.json.");
            }

            var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            return connection;
        }
    }
}
