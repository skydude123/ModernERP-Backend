
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using HRIStwinbeeAPI.DTO.Quotation;

namespace HRIStwinbeeAPI.Repo.Interface.Quotation;

public interface IQuotationRepository
{
    // ========================================================
    // HEADER
    // ========================================================

    Task<QuotationHeaderDto?> GetHeaderAsync(
        string sqNo,
        CancellationToken cancellationToken = default);

    // ========================================================
    // DETAILS
    // ========================================================

    Task<IReadOnlyList<QuotationDetailDto>> GetDetailsAsync(
        string sqNo,
        string vatType,
        CancellationToken cancellationToken = default);

    // ========================================================
    // ADDITIONAL CHARGES
    // ========================================================

    Task<QuotationAdditionalChargeDto?> GetAdditionalChargesAsync(
        string sqNo,
        CancellationToken cancellationToken = default);

    // ========================================================
    // TERMS
    // ========================================================

    Task<IReadOnlyList<QuotationTermDto>> GetTermsAsync(
        CancellationToken cancellationToken = default);

    // ========================================================
    // ARTISTS
    // ========================================================

    Task<IReadOnlyList<QuotationArtistDto>> GetArtistsAsync(
        string branchName,
        CancellationToken cancellationToken = default);

    // ========================================================
    // CUSTOMERS
    // ========================================================

    Task<IReadOnlyList<QuotationCustomerDto>> GetCustomersAsync(
        CancellationToken cancellationToken = default);

    // ========================================================
    // QUOTATION LIST
    // ========================================================

    Task<IReadOnlyList<QuotationListDto>> GetListAsync(
        string userId,
        string branchName,
        CancellationToken cancellationToken = default);

    // ========================================================
    // UNIT OF MEASURE
    // ========================================================

    Task<IReadOnlyList<string>> GetUnitsOfMeasureAsync(
        CancellationToken cancellationToken = default);

    // ========================================================
    // WARRANTY
    // ========================================================

    Task<IReadOnlyList<QuotationLookupOptionDto>> GetWarrantyAsync(
        CancellationToken cancellationToken = default);

    // ========================================================
    // DELIVERY
    // ========================================================

    Task<IReadOnlyList<QuotationLookupOptionDto>> GetDeliveryAsync(
        CancellationToken cancellationToken = default);

    // ========================================================
    // SERVICE LOOKUP — NEW
    // ========================================================

    Task<IReadOnlyList<QuotationServiceLookupDto>> GetServicesAsync(
        CancellationToken cancellationToken = default);

    // ========================================================
    // UNIFIED UPSERT — PRESERVED
    // ========================================================

    Task<QuotationUpsertResultDto> UpsertAsync(
        QuotationUpsertRequest request,
        string authenticatedUserId,
        string authenticatedBranch,
        bool isAdministrator,
        CancellationToken cancellationToken = default);


    // ==========================================================
    // CONFIRM QUOTATION
    // ==========================================================

    Task<QuotationConfirmResultDto> ConfirmAsync(
        string sqNo,
        string confirmRef,
        DateTime confirmDate,
        string authenticatedUserId,
        string authenticatedBranch,
        bool isAdministrator,
        CancellationToken cancellationToken = default);

}
