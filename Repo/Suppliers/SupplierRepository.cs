using System.Data;
using Dapper;
using HRIStwinbeeAPI.ConnectionData;
using HRIStwinbeeAPI.DTO.Suppliers;
using HRIStwinbeeAPI.Repo.Interface.Suppliers;
using Microsoft.Data.SqlClient;

namespace HRIStwinbeeAPI.Repo.Suppliers;

public sealed class SupplierRepository(
    ISqlConnectionFactory connectionFactory)
    : ISupplierRepository
{
    public async Task<SupplierPagedResult<SupplierListItemDto>> QueryAsync(
        SupplierListQueryRequest request,
        CancellationToken cancellationToken)
    {
        await using var connection =
            await connectionFactory.CreateOpenConnectionAsync(
                cancellationToken: cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("Search", request.Search, DbType.AnsiString, size: 100);
        parameters.Add("SupplierGroupCode", request.SupplierGroupCode, DbType.AnsiString, size: 5);
        parameters.Add("TermsCode", request.TermsCode, DbType.AnsiString, size: 4);
        parameters.Add("CurrencyCode", request.CurrencyCode, DbType.AnsiString, size: 5);
        parameters.Add("TaxProfileCode", request.TaxProfileCode, DbType.AnsiString, size: 15);
        parameters.Add("ActiveFlag", request.ActiveFlag, DbType.AnsiString, size: 3);
        parameters.Add("Skip", request.Skip, DbType.Int32);
        parameters.Add("Take", request.Take, DbType.Int32);
        parameters.Add("SortField", request.SortField, DbType.AnsiString, size: 40);
        parameters.Add("SortDirection", request.SortDirection, DbType.AnsiString, size: 4);

        var command = Command(
            "dbo.usp_Supplier_Query",
            parameters,
            cancellationToken);

        using var results = await connection.QueryMultipleAsync(command);
        var data = (await results.ReadAsync<SupplierListItemDto>()).AsList();
        var total = await results.ReadSingleAsync<long>();

        return new SupplierPagedResult<SupplierListItemDto>
        {
            Data = data,
            Total = total
        };
    }

    public async Task<SupplierDetailDto?> GetByCodeAsync(
        string supplierCode,
        CancellationToken cancellationToken)
    {
        await using var connection =
            await connectionFactory.CreateOpenConnectionAsync(
                cancellationToken: cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<SupplierDetailDto>(
            Command(
                "dbo.usp_Supplier_GetByCode",
                new { SupplierCode = supplierCode },
                cancellationToken));
    }

    public async Task<SupplierValidationResultDto> ValidateAsync(
        string supplierCode,
        SupplierWriteRequest request,
        bool isCreate,
        CancellationToken cancellationToken)
    {
        await using var connection =
            await connectionFactory.CreateOpenConnectionAsync(
                cancellationToken: cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("SupplierCode", supplierCode, DbType.AnsiString, size: 10);
        parameters.Add("SupplierName", request.SupplierName, DbType.AnsiString, size: 100);
        parameters.Add("TinNo", request.TinNo, DbType.AnsiString, size: 50);
        parameters.Add("SupplierGroupCode", request.SupplierGroupCode, DbType.AnsiString, size: 5);
        parameters.Add("TermsCode", request.TermsCode, DbType.AnsiString, size: 4);
        parameters.Add("CurrencyCode", request.CurrencyCode, DbType.AnsiString, size: 5);
        parameters.Add("TaxProfileCode", request.TaxProfileCode, DbType.AnsiString, size: 15);
        parameters.Add("IsCreate", isCreate, DbType.Boolean);

        return await connection.QuerySingleAsync<SupplierValidationResultDto>(
            Command(
                "dbo.usp_Supplier_Validate",
                parameters,
                cancellationToken));
    }

    public Task<SupplierCommandResultDto> CreateAsync(SupplierCreateRequest request,CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters();
        parameters.Add("SupplierCode", request.SupplierCode, DbType.AnsiString, size: 10);
        AddWriteParameters(parameters, request);
        parameters.Add("UserId", request.UserId, DbType.AnsiString, size: 10);

        return ExecuteCommandAsync(
            "dbo.usp_Supplier_Create",
            parameters,
            cancellationToken);
    }

    public Task<SupplierCommandResultDto> UpdateAsync(SupplierUpdateRequest request, CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters();
        parameters.Add("SupplierCode", request.SupplierCode, DbType.AnsiString, size: 10);
        AddWriteParameters(parameters, request);
        //parameters.Add("ExpectedModifiedDate", request.ExpectedModifiedDate, DbType.DateTime);
        parameters.Add("UserId", request.UserId, DbType.AnsiString, size: 10);

        return ExecuteCommandAsync(
            "dbo.usp_Supplier_Update",
            parameters,
            cancellationToken);
    }

    public Task<SupplierCommandResultDto> SetActiveStatusAsync(
        string supplierCode,
        string activeFlag,
        //DateTime expectedModifiedDate,
        string userId,
        CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters();
        parameters.Add("SupplierCode", supplierCode, DbType.AnsiString, size: 10);
        parameters.Add("ActiveFlag", activeFlag, DbType.AnsiStringFixedLength, size: 1);
        //parameters.Add("ExpectedModifiedDate", expectedModifiedDate, DbType.DateTime);
        parameters.Add("UserId", userId, DbType.AnsiString, size: 10);

        return ExecuteCommandAsync(
            "dbo.usp_Supplier_SetActiveStatus",
            parameters,
            cancellationToken);
    }

    public Task<IReadOnlyList<SupplierLookupDto>> GetGroupsAsync(
        string? search,
        CancellationToken cancellationToken) =>
        QueryLookupAsync(
            "dbo.usp_Supplier_GetGroups",
            search,
            cancellationToken);

    public Task<IReadOnlyList<SupplierLookupDto>> GetTermsAsync(
        string? search,
        CancellationToken cancellationToken) =>
        QueryLookupAsync(
            "dbo.usp_Supplier_GetTerms",
            search,
            cancellationToken);

    public Task<IReadOnlyList<SupplierLookupDto>> GetCurrenciesAsync(
        string? search,
        CancellationToken cancellationToken) =>
        QueryLookupAsync(
            "dbo.usp_Supplier_GetCurrencies",
            search,
            cancellationToken);

    public Task<IReadOnlyList<SupplierLookupDto>> GetTaxProfilesAsync(
        string? search,
        CancellationToken cancellationToken) =>
        QueryLookupAsync(
            "dbo.usp_Supplier_GetTaxProfiles",
            search,
            cancellationToken);

    private async Task<SupplierCommandResultDto> ExecuteCommandAsync(
        string procedure,
        DynamicParameters parameters,
        CancellationToken cancellationToken)
    {
        await using var connection =
            await connectionFactory.CreateOpenConnectionAsync(
                cancellationToken: cancellationToken);

        try
        {
            var row = await connection.QuerySingleAsync<SupplierCommandRow>(
                Command(procedure, parameters, cancellationToken));

            return new SupplierCommandResultDto
            {
                Supplier = ToDetail(row),
                Message = row.Message,
                WarningMessage = row.WarningMessage
            };
        }
        catch (SqlException exception) when (exception.Number >= 50000)
        {
            throw Translate(exception);
        }
    }

    private async Task<IReadOnlyList<SupplierLookupDto>> QueryLookupAsync(
        string procedure,
        string? search,
        CancellationToken cancellationToken)
    {
        await using var connection =
            await connectionFactory.CreateOpenConnectionAsync(
                cancellationToken: cancellationToken);

        var command = Command(
            procedure,
            new { Search = Normalize(search) },
            cancellationToken);

        return (await connection.QueryAsync<SupplierLookupDto>(command)).AsList();
    }

    private static void AddWriteParameters(
        DynamicParameters parameters,
        SupplierWriteRequest request)
    {
        parameters.Add("SupplierName", request.SupplierName, DbType.AnsiString, size: 100);
        parameters.Add("SupplierGroupCode", request.SupplierGroupCode, DbType.AnsiString, size: 5);
        parameters.Add("AddressLine1", request.AddressLine1, DbType.AnsiString, size: 200);
        parameters.Add("AddressLine2", request.AddressLine2, DbType.AnsiString, size: 35);
        parameters.Add("AddressLine3", request.AddressLine3, DbType.AnsiString, size: 35);
        parameters.Add("ContactPerson", request.ContactPerson, DbType.AnsiString, size: 35);
        parameters.Add("PositionTitle", request.PositionTitle, DbType.AnsiString, size: 25);
        parameters.Add("Telephone", request.Telephone, DbType.AnsiString, size: 30);
        parameters.Add("Fax", request.Fax, DbType.AnsiString, size: 30);
        parameters.Add("Email", request.Email, DbType.AnsiString, size: 45);
        parameters.Add("TinNo", request.TinNo, DbType.AnsiString, size: 50);
        parameters.Add("TermsCode", request.TermsCode, DbType.AnsiString, size: 4);
        parameters.Add("CurrencyCode", request.CurrencyCode, DbType.AnsiString, size: 5);
        parameters.Add("Website", request.Website, DbType.AnsiString, size: 80);
        parameters.Add("Specialties", request.Specialties, DbType.AnsiString, size: 500);
        parameters.Add("TaxProfileCode", request.TaxProfileCode, DbType.AnsiString, size: 15);
    }

    private static CommandDefinition Command(
        string procedure,
        object? parameters,
        CancellationToken cancellationToken) =>
        new(
            procedure,
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

    private static SupplierException Translate(SqlException exception)
    {
        var message = exception.Message;
        var detail = StripCode(message);

        if (message.StartsWith("SUPPLIER_NOT_FOUND:", StringComparison.OrdinalIgnoreCase))
        {
            return new SupplierNotFoundException(detail, exception);
        }

        if (
            message.StartsWith("SUPPLIER_DUPLICATE_", StringComparison.OrdinalIgnoreCase) ||
            message.StartsWith("SUPPLIER_CONCURRENCY:", StringComparison.OrdinalIgnoreCase) ||
            message.StartsWith("SUPPLIER_STATUS_CONFLICT:", StringComparison.OrdinalIgnoreCase))
        {
            return new SupplierConflictException(detail, exception);
        }

        return new SupplierValidationException(
            message.StartsWith("SUPPLIER_", StringComparison.OrdinalIgnoreCase)
                ? detail
                : "The supplier operation was rejected by the database.",
            exception);
    }

    private static string StripCode(string message)
    {
        var separator = message.IndexOf(':');
        return separator >= 0
            ? message[(separator + 1)..].Trim()
            : message;
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static SupplierDetailDto ToDetail(SupplierDetailDto row) =>
        new()
        {
            SupplierCode = row.SupplierCode,
            SupplierName = row.SupplierName,
            SupplierGroupCode = row.SupplierGroupCode,
            SupplierGroupName = row.SupplierGroupName,
            AddressLine1 = row.AddressLine1,
            AddressLine2 = row.AddressLine2,
            AddressLine3 = row.AddressLine3,
            ContactPerson = row.ContactPerson,
            PositionTitle = row.PositionTitle,
            Telephone = row.Telephone,
            Fax = row.Fax,
            Email = row.Email,
            TinNo = row.TinNo,
            TermsCode = row.TermsCode,
            TermsDescription = row.TermsDescription,
            CurrencyCode = row.CurrencyCode,
            CurrencyDescription = row.CurrencyDescription,
            Website = row.Website,
            Specialties = row.Specialties,
            TaxProfileCode = row.TaxProfileCode,
            TaxProfileDescription = row.TaxProfileDescription,
            ActiveFlag = row.ActiveFlag,
            ModifiedBy = row.ModifiedBy,
            //ModifiedDate = row.ModifiedDate
        };


}
