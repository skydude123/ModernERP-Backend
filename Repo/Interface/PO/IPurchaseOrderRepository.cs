using HRIStwinbeeAPI.DTO.PO;

namespace HRIStwinbeeAPI.Repo.Interface.PO
{
    public interface IPurchaseOrderRepository
    {
        Task<IReadOnlyList<SupplierLookupDto>> GetSuppliersAsync();
        Task<IReadOnlyList<BranchLookupDto>> GetBranchesAsync();
        Task<IReadOnlyList<TermLookupDto>> GetTermsAsync();
        Task<IReadOnlyList<VatTypeLookupDto>> GetVatTypesAsync();
        Task<IReadOnlyList<SoVerificationDto>> VerifySoNoAsync(string soNo);
        Task<IReadOnlyList<ItemLookupDto>> GetItemsAsync(string? search);
        Task<IReadOnlyList<UmDto>> GetUmAsync();

        Task<PurchaseOrderGetResponseDto> GetByNoAsync(string poNo);
        Task<PurchaseOrderSaveResponseDto> SaveAsync(
            PurchaseOrderSaveRequestDto request,
            CancellationToken cancellationToken = default);

        Task<PagedResultDto<PurchaseOrderListItemDto>> GetListPageAsync(PurchaseOrderListQueryDto query);

        Task<PurchaseOrderStatusActionResultDto> StatusActionAsync(PurchaseOrderStatusActionRequestDto request);

        Task<PurchaseOrderStatusActionResultDto> ReverseApprovalAsync(
     PurchaseOrderReverseApprovalRequestDto request,
     CancellationToken cancellationToken = default
 );
    }
}
