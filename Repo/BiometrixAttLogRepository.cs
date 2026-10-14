using Dapper;
using HRIStwinbeeAPI.ConnectionData;
using HRIStwinbeeAPI.DTO;
using HRIStwinbeeAPI.Repo.Interface;
using System.Data;

namespace HRIStwinbeeAPI.Repo
{
    public class BiometrixAttLogRepository : IBiometrixAttLogRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public BiometrixAttLogRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<long> SaveUploadBatchAsync(
            BiometrixSaveUploadBatchRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@OriginalFileName", request.OriginalFileName);
            parameters.Add("@DateFrom", request.DateFrom.Date);
            parameters.Add("@DateTo", request.DateTo.Date);
            parameters.Add("@FileSizeBytes", request.FileSizeBytes);
            parameters.Add("@RawLineCount", request.RawLineCount);
            parameters.Add("@ParserValidLineCount", request.ParserValidLineCount);
            parameters.Add("@ParserInvalidLineCount", request.ParserInvalidLineCount);
            parameters.Add("@UploadedBy", request.UploadedBy);
            parameters.Add("@RawPunches", request.RawPunchTable.AsTableValuedParameter("dbo.BiometrixRawPunchTvp"));
            parameters.Add("@UploadBatchId", dbType: DbType.Int64, direction: ParameterDirection.Output);
            parameters.Add("@payroll_period", request.PayrollPeriod);
            parameters.Add("@BranchName", request.BranchName);

            var command = new CommandDefinition(
                "dbo.usp_BiometrixAttLogUploadBatch_Save",
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await using var connection = await _connectionFactory.CreateOpenConnectionAsync(
                DbConnectionNames.Biometrix,
                cancellationToken);

            await connection.ExecuteAsync(command);

            return parameters.Get<long>("@UploadBatchId");
        }

        public async Task ProcessUploadBatchAsync(
            long uploadBatchId,
            CancellationToken cancellationToken = default)
        {
            var command = new CommandDefinition(
                "dbo.usp_BiometrixAttLogUploadBatch_Process",
                new { UploadBatchId = uploadBatchId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await using var connection = await _connectionFactory.CreateOpenConnectionAsync(
                DbConnectionNames.Biometrix,
                cancellationToken);

            await connection.ExecuteAsync(command);
        }

        public async Task<BiometrixUploadBatchDetailResponseDto?> GetBatchByIdAsync(
            long uploadBatchId,
            CancellationToken cancellationToken = default)
        {
            var command = new CommandDefinition(
                "dbo.usp_BiometrixAttLogUploadBatch_GetById",
                new { UploadBatchId = uploadBatchId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await using var connection = await _connectionFactory.CreateOpenConnectionAsync(
                DbConnectionNames.Biometrix,
                cancellationToken);

            using var grid = await connection.QueryMultipleAsync(command);

            var header = await grid.ReadFirstOrDefaultAsync<BiometrixUploadBatchDetailResponseDto>();

            if (header == null)
            {
                return null;
            }

            var rows = (await grid.ReadAsync<BiometrixAttendanceRowResponseDto>()).ToList();
            header.Rows = rows;

            return header;
        }

        public async Task<IReadOnlyList<BiometrixUploadBatchHistoryResponseDto>> GetHistoryAsync(
            int take,
            CancellationToken cancellationToken = default)
        {
            var command = new CommandDefinition(
                "dbo.usp_BiometrixAttLogUploadBatch_ListHistory",
                new { Take = take },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await using var connection = await _connectionFactory.CreateOpenConnectionAsync(
                DbConnectionNames.Biometrix,
                cancellationToken);

            var rows = await connection.QueryAsync<BiometrixUploadBatchHistoryResponseDto>(command);

            return rows.AsList();
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

            var rows = await connection.QueryAsync<BiometrixPayrollPeriodDto>(command);

            return rows.ToList();
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

            var rows = await connection.QueryAsync<BiometrixBranchDto>(command);

            return rows.ToList();
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