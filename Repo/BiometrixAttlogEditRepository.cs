using Dapper;
using HRIStwinbeeAPI.ConnectionData;
using HRIStwinbeeAPI.DTO;
using HRIStwinbeeAPI.Repo.Interface;
using System.Data;

namespace HRIStwinbeeAPI.Repo
{
    public class BiometrixAttlogEditRepository : IBiometrixAttlogEditRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public BiometrixAttlogEditRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IReadOnlyList<BiometrixPayrollPeriodDto>> GetPayrollPeriodsAsync(
            CancellationToken cancellationToken)
        {
            var command = new CommandDefinition(
                commandText: "dbo.sp_Biometrix_PayrollPeriod_GetList",
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await using var connection = await _connectionFactory.CreateOpenConnectionAsync(
                DbConnectionNames.Biometrix,
                cancellationToken);

            var result = await connection.QueryAsync<BiometrixPayrollPeriodDto>(command);

            return result.ToList();
        }

        public async Task<IReadOnlyList<BiometrixBranchDto>> GetBranchesAsync(
            CancellationToken cancellationToken)
        {
            var command = new CommandDefinition(
                commandText: "dbo.sp_Biometrix_Branch_GetList",
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await using var connection = await _connectionFactory.CreateOpenConnectionAsync(
                DbConnectionNames.Biometrix,
                cancellationToken);

            var result = await connection.QueryAsync<BiometrixBranchDto>(command);

            return result.ToList();
        }

        public async Task<BiometrixAttendanceOtUpdateResponseDto> UpdateAttendanceOtAsync(
            BiometrixAttendanceOtUpdateRequestDto request,
            CancellationToken cancellationToken)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@ProcessedAttendanceId", request.ProcessedAttendanceId, DbType.Int64);
            parameters.Add("@OtHours", request.OtHours, DbType.Decimal);
            parameters.Add("@UpdatedBy", request.UpdatedBy, DbType.String);

            var command = new CommandDefinition(
                commandText: "dbo.sp_Biometrix_ProcessedAttendance_UpdateOt",
                parameters: parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await using var connection = await _connectionFactory.CreateOpenConnectionAsync(
                DbConnectionNames.Biometrix,
                cancellationToken);

            return await connection.QuerySingleAsync<BiometrixAttendanceOtUpdateResponseDto>(command);
        }
    }
}