
using System.Data;
using Dapper;
using HRIStwinbeeAPI.ConnectionData;
using HRIStwinbeeAPI.Models.Security;
using Microsoft.Data.SqlClient;

namespace HRIStwinbeeAPI.Repo.Quotation
{
    public sealed class QuotationSecurityUserRepository
    {
        private readonly ISqlConnectionFactory
            _connectionFactory;

        public QuotationSecurityUserRepository(
            ISqlConnectionFactory connectionFactory
        )
        {
            _connectionFactory = connectionFactory;
        }

        private Task<SqlConnection> OpenConnectionAsync(
            CancellationToken cancellationToken = default
        )
        {
            return _connectionFactory.CreateOpenConnectionAsync(
                "QUOTATION",
                cancellationToken
            );
        }

        // ====================================================
        // GET ALL USERS
        // ====================================================

        public async Task<IEnumerable<SecurityUserDto>>
            GetUsersAsync(
                string? search,
                CancellationToken cancellationToken = default
            )
        {
            await using var connection =
                await OpenConnectionAsync(cancellationToken);

            var command = new CommandDefinition(
                commandText: "dbo.sp_Sec_User_List",
                parameters: new
                {
                    search
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken
            );

            return await connection.QueryAsync<SecurityUserDto>(
                command
            );
        }

        // ====================================================
        // GET ACTIVE MODULES
        // ====================================================

        public async Task<IEnumerable<SecurityModuleDto>>
            GetModulesAsync(
                CancellationToken cancellationToken = default
            )
        {
            await using var connection =
                await OpenConnectionAsync(cancellationToken);

            var command = new CommandDefinition(
                commandText: "dbo.sp_Sec_GetModules",
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken
            );

            return await connection.QueryAsync<SecurityModuleDto>(
                command
            );
        }

        // ====================================================
        // GET USER BY ID + MODULE ACCESS
        // ====================================================

        public async Task<SecurityUserDetailDto?>
            GetUserAsync(
                string userId,
                CancellationToken cancellationToken = default
            )
        {
            await using var connection =
                await OpenConnectionAsync(cancellationToken);

            var command = new CommandDefinition(
                commandText: "dbo.sp_Sec_User_GetById",
                parameters: new
                {
                    userId
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken
            );

            using var multi =
                await connection.QueryMultipleAsync(
                    command
                );

            var user =
                await multi
                    .ReadFirstOrDefaultAsync<SecurityUserDto>();

            if (user is null)
            {
                return null;
            }

            var modules =
                (await multi
                    .ReadAsync<SecurityModuleDto>())
                    .ToList();

            return new SecurityUserDetailDto
            {
                User = user,
                Modules = modules
            };
        }

        // ====================================================
        // CREATE OR UPDATE USER
        // ====================================================

        public async Task SaveAsync(
            string actorUserId,
            SaveSecurityUserRequest request,
            CancellationToken cancellationToken = default
        )
        {
            var moduleTable = new DataTable();

            moduleTable.Columns.Add(
                "module_code",
                typeof(string)
            );

            moduleTable.Columns.Add(
                "can_view",
                typeof(bool)
            );

            moduleTable.Columns.Add(
                "can_open",
                typeof(bool)
            );

            foreach (
                var module in request.Modules
                    ?? new List<SaveSecurityModuleRequest>()
            )
            {
                moduleTable.Rows.Add(
                    module.ModuleCode,
                    module.CanView,
                    module.CanOpen
                );
            }

            var parameters = new DynamicParameters();

            parameters.Add(
                "@actorUserId",
                actorUserId
            );

            parameters.Add(
                "@userId",
                request.UserId
            );

            parameters.Add(
                "@firstName",
                request.FirstName
            );

            parameters.Add(
                "@lastName",
                request.LastName
            );

            parameters.Add(
                "@password",
                request.Password
            );

            parameters.Add(
                "@designation",
                request.Designation
            );

            parameters.Add(
                "@administrator",
                request.Administrator
            );

            parameters.Add(
                "@branch",
                request.Branch
            );

            parameters.Add(
                "@stats",
                request.Stats
            );

            parameters.Add(
                "@DEPT",
                request.Dept
            );

            parameters.Add(
                "@entity",
                request.Entity
            );

            parameters.Add(
                "@userRights",
                request.UserRights
            );

            parameters.Add(
                "@modules",
                moduleTable.AsTableValuedParameter(
                    "dbo.SecUserModuleAccessType"
                )
            );

            await using var connection =
                await OpenConnectionAsync(cancellationToken);

            var command = new CommandDefinition(
                commandText: "dbo.sp_Sec_User_SaveUpsert",
                parameters: parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken
            );

            await connection.ExecuteAsync(
                command
            );
        }
    }
}
