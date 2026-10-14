using Dapper;
using HRIStwinbeeAPI.ConnectionData;
using HRIStwinbeeAPI.DTO;
using HRIStwinbeeAPI.Repo.Interface;
using System.Data;

namespace HRIStwinbeeAPI.Repo
{
    public class BiometrixDtrListingRepository : IBiometrixDtrListingRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public BiometrixDtrListingRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IReadOnlyList<BiometrixLookupItemDto>> GetBranchesAsync(
            CancellationToken cancellationToken = default)
        {
            var command = new CommandDefinition(
                "dbo.sp_Biometrix_Branch_GetList",
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await using var connection = await _connectionFactory.CreateOpenConnectionAsync(
                DbConnectionNames.Biometrix,
                cancellationToken);

            var rows = await connection.QueryAsync<string>(command);

            var result = new List<BiometrixLookupItemDto>
            {
                new BiometrixLookupItemDto
                {
                    Text = "ALL",
                    Value = "ALL"
                }
            };

            foreach (var item in rows)
            {
                var text = item?.Trim();

                if (!string.IsNullOrWhiteSpace(text) &&
                    !result.Any(x => x.Value.Equals(text, StringComparison.OrdinalIgnoreCase)))
                {
                    result.Add(new BiometrixLookupItemDto
                    {
                        Text = text,
                        Value = text
                    });
                }
            }

            return result;
        }

        public async Task<IReadOnlyList<BiometrixLookupItemDto>> GetPayrollPeriodsAsync(
            CancellationToken cancellationToken = default)
        {
            var command = new CommandDefinition(
                "dbo.sp_Biometrix_PayrollPeriod_GetList",
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await using var connection = await _connectionFactory.CreateOpenConnectionAsync(
                DbConnectionNames.Biometrix,
                cancellationToken);

            var rows = await connection.QueryAsync<int>(command);

            return rows
                .Distinct()
                .OrderByDescending(x => x)
                .Select(x => new BiometrixLookupItemDto
                {
                    Text = x.ToString(),
                    Value = x.ToString()
                })
                .ToList();
        }

        public async Task<IReadOnlyList<BiometrixDtrListingRowDto>> GetListingAsync(
            BiometrixDtrListingFilterDto filter,
            CancellationToken cancellationToken = default)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@BranchName", filter.BranchName);
            parameters.Add("@PayrollPeriod", filter.PayrollPeriod);

            var command = new CommandDefinition(
                "dbo.sp_Biometrix_DtrListing_GetList",
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await using var connection = await _connectionFactory.CreateOpenConnectionAsync(
                DbConnectionNames.Biometrix,
                cancellationToken);

            var rows = await connection.QueryAsync<BiometrixDtrListingRowDto>(command);

            return rows.AsList();
        }

        public async Task<BiometrixDtrListingRowDto?> UpdateAsync(
            BiometrixDtrListingUpdateDto request,
            CancellationToken cancellationToken = default)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@DtrId", request.DtrId);
            parameters.Add("@WorkDate", request.WorkDate.Date);
            parameters.Add("@ScheduleStartTime", request.ScheduleStartTime);
            parameters.Add("@ScheduleEndTime", request.ScheduleEndTime);
            parameters.Add("@TimeIn", request.TimeIn);
            parameters.Add("@TimeOut", request.TimeOut);
            parameters.Add("@NoOfHoursWork", request.NoOfHoursWork);
            parameters.Add("@OtHours", request.OtHours);
            parameters.Add("@Remarks", request.Remarks);


            var command = new CommandDefinition(
                "dbo.sp_Biometrix_DtrListing_Update",
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await using var connection = await _connectionFactory.CreateOpenConnectionAsync(
                DbConnectionNames.Biometrix,
                cancellationToken);

            return await connection.QueryFirstOrDefaultAsync<BiometrixDtrListingRowDto>(command);
        }
    }
}