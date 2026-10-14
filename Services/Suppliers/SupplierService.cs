using HRIStwinbeeAPI.DTO.Suppliers;
using HRIStwinbeeAPI.Repo.Interface.Suppliers;

namespace HRIStwinbeeAPI.Services.Suppliers;

public sealed class SupplierService(
    ISupplierRepository repository,
    ICurrentUserContext currentUserContext)
    : ISupplierService
{
    private static readonly IReadOnlyDictionary<string, string> SortFields =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["supplierCode"] = "SupplierCode",
            ["supplierName"] = "SupplierName",
            ["supplierGroupName"] = "SupplierGroupName",
            ["contactPerson"] = "ContactPerson",
            ["telephone"] = "Telephone",
            ["email"] = "Email",
            ["tinNo"] = "TinNo",
            ["termsCode"] = "TermsCode",
            ["currencyCode"] = "CurrencyCode",
            ["taxProfileCode"] = "TaxProfileCode",
            ["activeFlag"] = "ActiveFlag",
            ["modifiedBy"] = "ModifiedBy",
            ["modifiedDate"] = "ModifiedDate"
        };

    public Task<SupplierPagedResult<SupplierListItemDto>> QueryAsync(
        SupplierListQueryRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        request.Search = NormalizeOptional(request.Search, 100, "Search");
        request.SupplierGroupCode = NormalizeUpperOptional(
            request.SupplierGroupCode,
            SupplierFieldLengths.GroupCode,
            "Supplier group code");
        request.TermsCode = NormalizeUpperOptional(
            request.TermsCode,
            SupplierFieldLengths.TermsCode,
            "Terms code");
        request.CurrencyCode = NormalizeUpperOptional(
            request.CurrencyCode,
            SupplierFieldLengths.CurrencyCode,
            "Currency code");
        request.TaxProfileCode = NormalizeUpperOptional(
            request.TaxProfileCode,
            SupplierFieldLengths.TaxProfileCode,
            "Tax profile code");

        request.ActiveFlag = string.IsNullOrWhiteSpace(request.ActiveFlag)
            ? "Y"
            : request.ActiveFlag.Trim().ToUpperInvariant();

        if (request.ActiveFlag is not ("ALL" or "Y" or "N"))
        {
            throw new SupplierValidationException(
                "Active status must be Active, Inactive, or All.");
        }

        request.Skip = Math.Max(0, request.Skip);
        request.Take = Math.Clamp(request.Take, 1, 250);
        request.SortField = SortFields.TryGetValue(
            request.SortField?.Trim() ?? string.Empty,
            out var sortField)
            ? sortField
            : "SupplierName";
        request.SortDirection = string.Equals(
            request.SortDirection,
            "desc",
            StringComparison.OrdinalIgnoreCase)
            ? "DESC"
            : "ASC";

        return repository.QueryAsync(request, cancellationToken);
    }

    public Task<SupplierDetailDto?> GetByCodeAsync(
        string supplierCode,
        CancellationToken cancellationToken) =>
        repository.GetByCodeAsync(
            NormalizeSupplierCode(supplierCode),
            cancellationToken);

    public async Task<SupplierCommandResultDto> CreateAsync(
        SupplierCreateRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        request.SupplierCode = NormalizeSupplierCode(request.SupplierCode);
        NormalizeWriteRequest(request);

        var validation = await repository.ValidateAsync(
            request.SupplierCode,
            request,
            isCreate: true,
            cancellationToken);

        EnsureValid(validation, isCreate: true);

        var result = await repository.CreateAsync(
            request,
            //currentUserContext.GetRequiredUserId(),
            cancellationToken);

        result.WarningMessage ??= validation.WarningMessage;
        return result;
    }

    public async Task<SupplierCommandResultDto> UpdateAsync(
        string supplierCode,
        SupplierUpdateRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var routeCode = NormalizeSupplierCode(supplierCode);
        var bodyCode = NormalizeSupplierCode(request.SupplierCode);

        if (!string.Equals(routeCode, bodyCode, StringComparison.Ordinal))
        {
            throw new SupplierValidationException(
                "Supplier code is immutable and must match the route value.");
        }

        //request.ExpectedModifiedDate = RequireConcurrencyToken(
        //    request.ExpectedModifiedDate,
        //    "updating a supplier");

        request.SupplierCode = routeCode;
        NormalizeWriteRequest(request);

        var validation = await repository.ValidateAsync(
            routeCode,
            request,
            isCreate: false,
            cancellationToken);

        EnsureValid(validation, isCreate: false);

        var result = await repository.UpdateAsync(
            request,
         //   currentUserContext.GetRequiredUserId(),
            cancellationToken);

        result.WarningMessage ??= validation.WarningMessage;
        return result;
    }

    public async Task<SupplierCommandResultDto> SetActiveStatusAsync(
        string supplierCode,
        string activeFlag,
       // SupplierStatusRequest request,
        CancellationToken cancellationToken)
    {
       // ArgumentNullException.ThrowIfNull(request);

        var code = NormalizeSupplierCode(supplierCode);
        var targetStatus = activeFlag.Trim().ToUpperInvariant();

        if (targetStatus is not ("Y" or "N"))
        {
            throw new SupplierValidationException(
                "Supplier status must be Y or N.");
        }

        //request.ExpectedModifiedDate = RequireConcurrencyToken(
        //    request.ExpectedModifiedDate,
        //    "changing supplier status");

        var existing = await repository.GetByCodeAsync(code, cancellationToken)
            ?? throw new SupplierNotFoundException(
                $"Supplier '{code}' was not found.");

        //if (existing.ModifiedDate != request.ExpectedModifiedDate.Value)
        //{
        //    throw new SupplierConflictException(
        //        "The supplier was changed by another user. Refresh before trying again.");
        //}

        if (existing.ActiveFlag == targetStatus)
        {
            throw new SupplierConflictException(
                targetStatus == "Y"
                    ? "The supplier is already active."
                    : "The supplier is already inactive.");
        }

        return await repository.SetActiveStatusAsync(
            code,
            targetStatus,
      //      request.ExpectedModifiedDate.Value,
            currentUserContext.GetRequiredUserId(),
            cancellationToken);
    }

    public Task<IReadOnlyList<SupplierLookupDto>> GetGroupsAsync(
        string? search,
        CancellationToken cancellationToken) =>
        repository.GetGroupsAsync(
            NormalizeOptional(search, 100, "Search"),
            cancellationToken);

    public Task<IReadOnlyList<SupplierLookupDto>> GetTermsAsync(
        string? search,
        CancellationToken cancellationToken) =>
        repository.GetTermsAsync(
            NormalizeOptional(search, 100, "Search"),
            cancellationToken);

    public Task<IReadOnlyList<SupplierLookupDto>> GetCurrenciesAsync(
        string? search,
        CancellationToken cancellationToken) =>
        repository.GetCurrenciesAsync(
            NormalizeOptional(search, 100, "Search"),
            cancellationToken);

    public Task<IReadOnlyList<SupplierLookupDto>> GetTaxProfilesAsync(
        string? search,
        CancellationToken cancellationToken) =>
        repository.GetTaxProfilesAsync(
            NormalizeOptional(search, 100, "Search"),
            cancellationToken);

    private static void EnsureValid(
        SupplierValidationResultDto result,
        bool isCreate)
    {
        if (isCreate && result.SupplierCodeExists)
        {
            throw new SupplierConflictException(
                "Supplier code already exists.");
        }

        if (!isCreate && !result.SupplierExists)
        {
            throw new SupplierNotFoundException(
                "The supplier no longer exists.");
        }

        if (!result.SupplierGroupExists)
        {
            throw new SupplierValidationException(
                "The selected supplier group does not exist.");
        }

        if (!result.TermsExists)
        {
            throw new SupplierValidationException(
                "The selected payment terms code does not exist.");
        }

        if (!result.CurrencyExists)
        {
            throw new SupplierValidationException(
                "The selected currency code does not exist.");
        }

        if (!result.TaxProfileExists)
        {
            throw new SupplierValidationException(
                "The selected tax profile is not active or does not exist.");
        }

        if (result.DuplicateTinExists)
        {
            throw new SupplierConflictException(
                "Another supplier already uses the same normalized TIN.");
        }
    }

    private static void NormalizeWriteRequest(SupplierWriteRequest request)
    {
        request.SupplierName = NormalizeRequired(
            request.SupplierName,
            SupplierFieldLengths.Name,
            "Supplier name");
        request.SupplierGroupCode = NormalizeRequired(
            request.SupplierGroupCode,
            SupplierFieldLengths.GroupCode,
            "Supplier group code").ToUpperInvariant();
        request.AddressLine1 = NormalizeOptional(request.AddressLine1, SupplierFieldLengths.AddressLine1, "Address line 1");
        request.AddressLine2 = NormalizeOptional(request.AddressLine2, SupplierFieldLengths.AddressLine2, "Address line 2");
        request.AddressLine3 = NormalizeOptional(request.AddressLine3, SupplierFieldLengths.AddressLine3, "Address line 3");
        request.ContactPerson = NormalizeOptional(request.ContactPerson, SupplierFieldLengths.ContactPerson, "Contact person");
        request.PositionTitle = NormalizeOptional(request.PositionTitle, SupplierFieldLengths.PositionTitle, "Position/title");
        request.Telephone = NormalizeOptional(request.Telephone, SupplierFieldLengths.Telephone, "Telephone");
        request.Fax = NormalizeOptional(request.Fax, SupplierFieldLengths.Fax, "Fax");
        request.Email = NormalizeOptional(request.Email, SupplierFieldLengths.Email, "Email");
        request.TinNo = NormalizeOptional(request.TinNo, SupplierFieldLengths.TinNo, "TIN");
        request.TermsCode = NormalizeUpperOptional(request.TermsCode, SupplierFieldLengths.TermsCode, "Terms code");
        request.CurrencyCode = NormalizeUpperOptional(request.CurrencyCode, SupplierFieldLengths.CurrencyCode, "Currency code");
        request.Website = NormalizeOptional(request.Website, SupplierFieldLengths.Website, "Website");
        request.Specialties = NormalizeOptional(request.Specialties, SupplierFieldLengths.Specialties, "Specialties");
        request.TaxProfileCode = NormalizeUpperOptional(request.TaxProfileCode, SupplierFieldLengths.TaxProfileCode, "Tax profile code");
    }

    private static string NormalizeSupplierCode(string? value) =>
        NormalizeRequired(
            value,
            SupplierFieldLengths.Code,
            "Supplier code").ToUpperInvariant();

    private static DateTime RequireConcurrencyToken(
        DateTime? value,
        string operation)
    {
        var minimumSqlDateTime = new DateTime(1753, 1, 1);

        if (!value.HasValue || value.Value < minimumSqlDateTime)
        {
            throw new SupplierValidationException(
                $"Expected modified date is required when {operation}.");
        }

        return value.Value;
    }

    private static string NormalizeRequired(
        string? value,
        int maximumLength,
        string fieldName)
    {
        var normalized = value?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new SupplierValidationException(
                $"{fieldName} is required.");
        }

        EnsureLength(normalized, maximumLength, fieldName);
        return normalized;
    }

    private static string? NormalizeUpperOptional(
        string? value,
        int maximumLength,
        string fieldName) =>
        NormalizeOptional(value, maximumLength, fieldName)?.ToUpperInvariant();

    private static string? NormalizeOptional(
        string? value,
        int maximumLength,
        string fieldName)
    {
        var normalized = value?.Trim();

        if (string.IsNullOrEmpty(normalized))
        {
            return null;
        }

        EnsureLength(normalized, maximumLength, fieldName);
        return normalized;
    }

    private static void EnsureLength(
        string value,
        int maximumLength,
        string fieldName)
    {
        if (value.Length > maximumLength)
        {
            throw new SupplierValidationException(
                $"{fieldName} cannot exceed {maximumLength} characters.");
        }
    }
}
