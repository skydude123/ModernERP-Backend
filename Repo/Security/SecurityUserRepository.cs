using System.Data;

using Dapper;
using Microsoft.Data.SqlClient;

using HRIStwinbeeAPI.Models.Security;

namespace HRIStwinbeeAPI.Repositories.Security
{
    public sealed class SecurityUserRepository
    {
        private readonly string _connectionString;

        public SecurityUserRepository(
            IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("TWINBEE")
                ?? throw new InvalidOperationException(
                    "Database connection string was not found."
                );
        }


        private SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }


        /* ============================================================
           GET USERS
        ============================================================ */
        public async Task<IEnumerable<SecurityUserDto>> GetUsersAsync(
            string? search)
        {
            await using var connection =
                CreateConnection();

            return await connection.QueryAsync<SecurityUserDto>(
                "dbo.sp_Sec_User_List",
                new
                {
                    search
                },
                commandType:
                    CommandType.StoredProcedure
            );
        }


        /* ============================================================
           GET ALL SECURITY MODULES
        ============================================================ */
        public async Task<IEnumerable<SecurityModuleDto>> GetModulesAsync()
        {
            await using var connection =
                CreateConnection();

            return await connection.QueryAsync<SecurityModuleDto>(
                "dbo.sp_Sec_GetModules",
                commandType:
                    CommandType.StoredProcedure
            );
        }


        /* ============================================================
           GET ONE USER + MODULE ACCESS
        ============================================================ */
        public async Task<SecurityUserDetailDto?> GetUserAsync(
            string userId)
        {
            await using var connection =
                CreateConnection();

            using var multi =
                await connection.QueryMultipleAsync(
                    "dbo.sp_Sec_User_GetById",
                    new
                    {
                        userId
                    },
                    commandType:
                        CommandType.StoredProcedure
                );


            var user =
                await multi.ReadFirstOrDefaultAsync<SecurityUserDto>();

            if (user is null)
            {
                return null;
            }


            var modules =
                (await multi.ReadAsync<SecurityModuleDto>())
                .ToList();


            return new SecurityUserDetailDto
            {
                User = user,
                Modules = modules
            };
        }


        /* ============================================================
           CREATE / UPDATE USER + MODULE ACCESS
        ============================================================ */
        public async Task SaveAsync(
            string actorUserId,
            SaveSecurityUserRequest request)
        {
            var moduleTable =
                new DataTable();


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


            foreach (var module in request.Modules)
            {
                moduleTable.Rows.Add(
                    module.ModuleCode,
                    module.CanView,
                    module.CanOpen
                );
            }


            var parameters =
                new DynamicParameters();


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
                CreateConnection();


            await connection.ExecuteAsync(
                "dbo.sp_Sec_User_SaveUpsert",
                parameters,
                commandType:
                    CommandType.StoredProcedure
            );
        }
    }
}