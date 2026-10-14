using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;

namespace twbERRANDSapi.AUTH
{
    public sealed class AuthRepository : IAuthRepository
    {
        private readonly IDbConnection _db;
        private readonly ILogger<AuthRepository> _logger;

        public AuthRepository(IDbConnection db, ILogger<AuthRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@userId", request.UserId, DbType.String);
                parameters.Add("@password", request.Password, DbType.String);

                var command = new CommandDefinition(
                    commandText: "dbo.sp_Sec_LoginWithPermissions",
                    parameters: parameters,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken
                );

                using var multi = await _db.QueryMultipleAsync(command);

                var user = await multi.ReadFirstOrDefaultAsync<AuthUserDto>();
                if (user is null)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid user ID or password.",
                        User = null
                    };
                }

                var permissions = (await multi.ReadAsync<UserPermissionDto>()).ToList();
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
                _logger.LogError(ex, "Login failed for UserId: {UserId}", request.UserId);

                return new LoginResponse
                {
                    Success = false,
                    Message = "An unexpected error occurred while processing the login request.",
                    User = null
                };
            }
        }


        //public async Task<LoginUserDto?> LoginAsync(string userId, string password, CancellationToken cancellationToken = default)
        //{
        //    var cmd = new CommandDefinition(
        //        commandText: "dbo.sp_user_login",
        //        parameters: new
        //        {
        //            userid = userId,
        //            password = password
        //        },
        //        commandType: CommandType.StoredProcedure,
        //        cancellationToken: cancellationToken
        //    );

        //    return await _db.QueryFirstOrDefaultAsync<LoginUserDto>(cmd);
        //}
    }
}
