
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Dapper;

using HRIStwinbeeAPI.ConnectionData;
using HRIStwinbeeAPI.Repo.Interface.Quotation;

using Microsoft.Extensions.Logging;

using twbERRANDSapi.AUTH;

namespace HRIStwinbeeAPI.Repo.Quotation
{
    public sealed class QuotationAuthRepository
        : IQuotationAuthRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        private readonly ILogger<QuotationAuthRepository> _logger;

        public QuotationAuthRepository(
            ISqlConnectionFactory connectionFactory,
            ILogger<QuotationAuthRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        // =========================================================
        // LOGIN USING TWINBEE DATABASE
        // =========================================================

        public async Task<LoginResponse> LoginAsync(
            LoginRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.UserId) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid user ID or password.",
                    User = null
                };
            }

            try
            {
                await using var connection =
                    await _connectionFactory
                        .CreateOpenConnectionAsync(
                            "TWINBEE",
                            cancellationToken);

                var parameters = new DynamicParameters();

                parameters.Add(
                    "@userId",
                    request.UserId.Trim(),
                    DbType.AnsiString,
                    size: 10);

                parameters.Add(
                    "@password",
                    request.Password,
                    DbType.AnsiString,
                    size: 20);

                var command = new CommandDefinition(
                    commandText:
                        "dbo.sp_Sec_LoginWithPermissions",
                    parameters: parameters,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken);

                using var resultSet =
                    await connection.QueryMultipleAsync(command);

                // RESULT SET 1: USER

                var user =
                    await resultSet
                        .ReadFirstOrDefaultAsync<AuthUserDto>();

                if (user == null)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid user ID or password.",
                        User = null
                    };
                }

                // RESULT SET 2: EFFECTIVE PERMISSIONS

                var permissions =
                    (await resultSet
                        .ReadAsync<UserPermissionDto>())
                        .ToList();

                user.Permissions = permissions;

                return new LoginResponse
                {
                    Success = true,
                    Message = "Login successful.",
                    User = user
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Quotation login repository failed. UserId={UserId}",
                    request.UserId);

                throw;
            }
        }
    }
}
