
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using HRIStwinbeeAPI.DTO.Quotation;
using HRIStwinbeeAPI.Repo.Interface.Quotation;

namespace HRIStwinbeeAPI.Services.Quotation;

public sealed class QuotationService : IQuotationService
{
    private readonly IQuotationRepository _repository;

    public QuotationService(
        IQuotationRepository repository)
    {
        _repository = repository;
    }

    // ==========================================================
    // HEADER
    // ==========================================================

    public Task<QuotationHeaderDto?> GetHeaderAsync(
        string sqNo,
        CancellationToken cancellationToken = default)
    {
        return _repository.GetHeaderAsync(
            sqNo.Trim(),
            cancellationToken);
    }

    // ==========================================================
    // DETAILS
    // ==========================================================

    public async Task<IReadOnlyList<QuotationDetailDto>>
        GetDetailsAsync(
            string sqNo,
            CancellationToken cancellationToken = default)
    {
        sqNo = sqNo.Trim();

        var header = await _repository.GetHeaderAsync(
            sqNo,
            cancellationToken);

        if (header is null)
        {
            throw new KeyNotFoundException(
                "Quotation was not found.");
        }

        return await _repository.GetDetailsAsync(
            sqNo,
            header.VatType?.Trim() ?? "EXCLUDED",
            cancellationToken);
    }

    // ==========================================================
    // ADDITIONAL CHARGES
    // ==========================================================

    public Task<QuotationAdditionalChargeDto?>
        GetAdditionalChargesAsync(
            string sqNo,
            CancellationToken cancellationToken = default)
    {
        return _repository.GetAdditionalChargesAsync(
            sqNo.Trim(),
            cancellationToken);
    }

    // ==========================================================
    // COMPLETE QUOTATION
    // ==========================================================

    public async Task<QuotationReadDto?> GetQuotationAsync(
        string sqNo,
        CancellationToken cancellationToken = default)
    {
        sqNo = sqNo.Trim();

        var header = await _repository.GetHeaderAsync(
            sqNo,
            cancellationToken);

        if (header is null)
        {
            return null;
        }

        var details = await _repository.GetDetailsAsync(
            sqNo,
            header.VatType?.Trim() ?? "EXCLUDED",
            cancellationToken);

        var additional =
            await _repository.GetAdditionalChargesAsync(
                sqNo,
                cancellationToken);

        return new QuotationReadDto
        {
            Header = header,
            Details = details,
            AdditionalCharges = additional
        };
    }

    // ==========================================================
    // TERMS
    // ==========================================================

    public Task<IReadOnlyList<QuotationTermDto>>
        GetTermsAsync(
            CancellationToken cancellationToken = default)
    {
        return _repository.GetTermsAsync(
            cancellationToken);
    }

    // ==========================================================
    // ARTISTS
    // ==========================================================

    public Task<IReadOnlyList<QuotationArtistDto>>
        GetArtistsAsync(
            string branchName,
            CancellationToken cancellationToken = default)
    {
        return _repository.GetArtistsAsync(
            branchName.Trim(),
            cancellationToken);
    }

    // ==========================================================
    // CUSTOMERS
    // ==========================================================

    public Task<IReadOnlyList<QuotationCustomerDto>>
        GetCustomersAsync(
            CancellationToken cancellationToken = default)
    {
        return _repository.GetCustomersAsync(
            cancellationToken);
    }

    // ==========================================================
    // LIST
    // ==========================================================

    public Task<IReadOnlyList<QuotationListDto>>
        GetListAsync(
            string userId,
            string branchName,
            CancellationToken cancellationToken = default)
    {
        return _repository.GetListAsync(
            userId.Trim(),
            branchName.Trim(),
            cancellationToken);
    }

    // ==========================================================
    // UNIT OF MEASURE
    // ==========================================================

    public Task<IReadOnlyList<string>>
        GetUnitsOfMeasureAsync(
            CancellationToken cancellationToken = default)
    {
        return _repository.GetUnitsOfMeasureAsync(
            cancellationToken);
    }

    // ==========================================================
    // WARRANTY
    // ==========================================================

    public Task<IReadOnlyList<QuotationLookupOptionDto>>
        GetWarrantyAsync(
            CancellationToken cancellationToken = default)
    {
        return _repository.GetWarrantyAsync(
            cancellationToken);
    }

    // ==========================================================
    // DELIVERY
    // ==========================================================

    public Task<IReadOnlyList<QuotationLookupOptionDto>>
        GetDeliveryAsync(
            CancellationToken cancellationToken = default)
    {
        return _repository.GetDeliveryAsync(
            cancellationToken);
    }


    // ==========================================================
    // SERVICE LOOKUP
    // ==========================================================

    public Task<IReadOnlyList<QuotationServiceLookupDto>>
        GetServicesAsync(
            CancellationToken cancellationToken = default)
    {
        return _repository.GetServicesAsync(
            cancellationToken);
    }


    // ==========================================================
    // HELPER: CHECK STRING LENGTH
    // ==========================================================

    private static void CheckLength(
        string? value,
        int maximum,
        string field)
    {
        if (value is not null &&
            value.Length > maximum)
        {
            throw new ArgumentException(
                $"{field} cannot exceed {maximum} characters.");
        }
    }


    // ==========================================================
    // CONFIRM QUOTATION
    // VALIDATION + REPOSITORY CALL
    // ==========================================================

    public async Task<QuotationConfirmResultDto> ConfirmAsync(
        string sqNo,
        QuotationConfirmRequestDto request,
        string authenticatedUserId,
        string authenticatedBranch,
        bool isAdministrator,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sqNo) ||
            sqNo.Trim().Length != 10 ||
            !sqNo.Trim().All(char.IsDigit))
        {
            throw new ArgumentException(
                "Invalid quotation number.");
        }

        if (request is null)
        {
            throw new ArgumentException(
                "Confirmation information is required.");
        }

        var confirmRef = request.ConfirmRef?.Trim() ?? "";

        if (confirmRef.Length == 0)
        {
            throw new ArgumentException(
                "Confirmation reference is required.");
        }

        if (confirmRef.Length > 50)
        {
            throw new ArgumentException(
                "Confirmation reference must not exceed 50 characters.");
        }

        if (!request.ConfirmDate.HasValue)
        {
            throw new ArgumentException(
                "Confirmation date is required.");
        }

        var confirmDate = request.ConfirmDate.Value.Date;

        if (string.IsNullOrWhiteSpace(authenticatedUserId))
        {
            throw new UnauthorizedAccessException(
                "Authenticated user is missing.");
        }

        if (!isAdministrator &&
            string.IsNullOrWhiteSpace(authenticatedBranch))
        {
            throw new UnauthorizedAccessException(
                "Authenticated branch is missing.");
        }

        return await _repository.ConfirmAsync(
            sqNo.Trim(),
            confirmRef,
            confirmDate,
            authenticatedUserId,
            authenticatedBranch ?? "",
            isAdministrator,
            cancellationToken);
    }


    // ==========================================================
    // SINGLE QUOTATION UPSERT
    // INSERT / UPDATE
    // ==========================================================

    public async Task<QuotationUpsertResultDto> UpsertAsync(
        QuotationUpsertRequest request,
        string authenticatedUserId,
        string authenticatedBranch,
        bool isAdministrator,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentException(
                "Quotation request is required.");
        }

        // ======================================================
        // NORMALIZE
        // ======================================================

        request.Action =
            (request.Action ?? "")
            .Trim()
            .ToUpperInvariant();

        request.SaveScope =
            (request.SaveScope ?? "")
            .Trim()
            .ToUpperInvariant();

        request.SqNo =
            (request.SqNo ?? "").Trim();

        authenticatedUserId =
            (authenticatedUserId ?? "").Trim();

        authenticatedBranch =
            (authenticatedBranch ?? "").Trim();

        // ======================================================
        // VALIDATE ACTION
        // ======================================================

        if (request.Action != "INSERT" &&
            request.Action != "UPDATE")
        {
            throw new ArgumentException(
                "Action must be INSERT or UPDATE.");
        }

        // ======================================================
        // VALIDATE SAVE SCOPE
        // ======================================================

        if (request.SaveScope != "HEADER" &&
            request.SaveScope != "DETAILS" &&
            request.SaveScope != "ALL")
        {
            throw new ArgumentException(
                "Invalid quotation save scope.");
        }

        // ======================================================
        // INSERT / UPDATE RULES
        // ======================================================

        if (request.Action == "INSERT")
        {
            if (request.SaveScope != "ALL")
            {
                throw new ArgumentException(
                    "New quotation requires ALL save scope.");
            }

            if (!string.IsNullOrWhiteSpace(request.SqNo))
            {
                throw new ArgumentException(
                    "Quotation number must be empty for INSERT.");
            }
        }
        else
        {
            if (request.SqNo.Length != 10 ||
                !request.SqNo.All(char.IsDigit))
            {
                throw new ArgumentException(
                    "UPDATE requires a valid 10-digit quotation number.");
            }
        }

        // ======================================================
        // AUTHENTICATED SESSION
        // ======================================================

        if (string.IsNullOrWhiteSpace(authenticatedUserId) ||
            string.IsNullOrWhiteSpace(authenticatedBranch))
        {
            throw new UnauthorizedAccessException(
                "Quotation session is invalid.");
        }

        if (authenticatedUserId.Length > 10 ||
            authenticatedBranch.Length > 15)
        {
            throw new ArgumentException(
                "Invalid user or branch length.");
        }

        // ======================================================
        // HEADER VALIDATION
        // ======================================================

        if (request.SaveScope is "HEADER" or "ALL")
        {
            CheckLength(request.ProcessId, 3, "Process");
            CheckLength(request.VatType, 20, "VAT Type");
            CheckLength(request.TermId, 3, "Terms");
            CheckLength(request.Warranty, 20, "Warranty");
            CheckLength(request.Delivery, 20, "Delivery");
            CheckLength(request.AcPerson, 30, "Account Person");
            CheckLength(request.ContactP, 60, "Contact Person");
            CheckLength(request.Remarks, 400, "Remarks");

            if (request.CustomerId.HasValue &&
                request.CustomerId.Value <= 0)
            {
                throw new ArgumentException(
                    "Invalid customer ID.");
            }

            if (request.ArtCode.HasValue &&
                request.ArtCode.Value <= 0)
            {
                throw new ArgumentException(
                    "Invalid artist code.");
            }

            // Discount 0.02 = 2%.
            if (request.Discnt.HasValue)
            {
                if (request.Discnt.Value < 0m ||
                    request.Discnt.Value > 1m)
                {
                    throw new ArgumentException(
                        "Discount must be between 0% and 100%.");
                }

                if (decimal.Round(request.Discnt.Value, 6)
                    != request.Discnt.Value)
                {
                    throw new ArgumentException(
                        "Discount exceeds six decimal places.");
                }
            }

            if (request.SqDate.HasValue)
            {
                var date = request.SqDate.Value;

                if (date.Year < 1900 ||
                    date.Year > 2079)
                {
                    throw new ArgumentException(
                        "Quotation date is outside supported range.");
                }
            }
        }

        // ======================================================
        // ADDITIONAL INSERT VALIDATION
        // ======================================================

        if (request.Action == "INSERT")
        {
            if (!request.CustomerId.HasValue ||
                request.CustomerId.Value <= 0)
            {
                throw new ArgumentException(
                    "Customer is required.");
            }

            if (!request.SqDate.HasValue)
            {
                throw new ArgumentException(
                    "Quotation date is required.");
            }

            request.VatType =
                (request.VatType ?? "")
                .Trim()
                .ToUpperInvariant();

            if (request.VatType != "EXCLUDED" &&
                request.VatType != "EXCLUSIVE" &&
                request.VatType != "INCLUSIVE")
            {
                throw new ArgumentException(
                    "Invalid VAT Type.");
            }

            request.TermId =
                (request.TermId ?? "").Trim();

            if (request.TermId.Length == 0 ||
                request.TermId.Length > 3)
            {
                throw new ArgumentException(
                    "A valid Terms ID is required.");
            }

            request.Discnt ??= 0m;
        }

        // ======================================================
        // DETAILS VALIDATION
        // ======================================================

        request.Details ??=
            new List<QuotationUpsertDetailDto>();

        if (request.SaveScope is "DETAILS" or "ALL")
        {
            if (request.Details.Count == 0)
            {
                throw new ArgumentException(
                    "At least one quotation detail is required.");
            }

            var sequences = new HashSet<int>();

            foreach (var item in request.Details)
            {
                if (item is null)
                {
                    throw new ArgumentException(
                        "Quotation detail cannot be null.");
                }

                if (item.SeqLineNo <= 0 ||
                    !sequences.Add(item.SeqLineNo))
                {
                    throw new ArgumentException(
                        "Invalid or duplicate detail sequence number.");
                }

                item.Description =
                    (item.Description ?? "").Trim();

                if (item.Description.Length == 0 ||
                    item.Description.Length > 250)
                {
                    throw new ArgumentException(
                        $"Invalid description on line {item.SeqLineNo}.");
                }

                CheckLength(item.Size, 50, "Size");
                CheckLength(item.UM, 20, "Unit of Measure");
                CheckLength(item.ServiceId, 25, "Service ID");
                CheckLength(item.FinalFin, 255, "Finishing");

                CheckLength(item.Materials, 4000, "Materials");

                // Legacy quantity NUMERIC(9,0).
                if (item.Qty <= 0 ||
                    item.Qty > 999999999m ||
                    item.Qty != decimal.Truncate(item.Qty))
                {
                    throw new ArgumentException(
                        $"Invalid quantity on line {item.SeqLineNo}.");
                }

                // UnitCost NUMERIC(12,5).
                if (item.UnitCost < 0 ||
                    item.UnitCost > 9999999.99999m ||
                    decimal.Round(item.UnitCost, 5)
                        != item.UnitCost)
                {
                    throw new ArgumentException(
                        $"Invalid unit cost on line {item.SeqLineNo}.");
                }

                // Total NUMERIC(12,2).
                if (item.TotAmt < 0 ||
                    item.TotAmt > 9999999999.99m ||
                    decimal.Round(item.TotAmt, 2)
                        != item.TotAmt)
                {
                    throw new ArgumentException(
                        $"Invalid amount on line {item.SeqLineNo}.");
                }

                // Legacy detail VAT/Discount NUMERIC(9,0).
                if (item.VAT < 0m ||
    item.VAT > 9999999999.99m ||
    decimal.Round(item.VAT, 2) != item.VAT ||
    item.Discnt < 0m ||
    item.Discnt > 9999999999.99m ||
    decimal.Round(item.Discnt, 2) != item.Discnt)
                {
                    throw new ArgumentException(
                        $"Line {item.SeqLineNo}: VAT and discount " +
                        "must be valid amounts with up to 2 decimal places.");
                }
            }
        }
        else
        {
            // HEADER save must never change details.
            request.Details.Clear();
        }

        // ======================================================
        // SINGLE REPOSITORY SAVE
        // ======================================================

        return await _repository.UpsertAsync(
            request,
            authenticatedUserId,
            authenticatedBranch,
            isAdministrator,
            cancellationToken);
    }
}
