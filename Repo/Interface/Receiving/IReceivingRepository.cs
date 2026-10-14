using HRIStwinbeeAPI.DTO.Receiving;

namespace HRIStwinbeeAPI.Repo.Interface.Receiving
{
    public interface IReceivingRepository
    {
        Task<PagedResult<ReceivingListItemDto>> QueryAsync(
      ReceivingQueryRequest request,
      CancellationToken cancellationToken);

        Task<ReceivingDocumentDto?> GetByNoAsync(
            string rrNo,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<SupplierLookupDto>> GetSuppliersAsync(
            CancellationToken cancellationToken);

    //    Task<IReadOnlyList<SoVerificationDto>> VerifySoNoAsync(
    //string soNo,
    //CancellationToken cancellationToken);

        Task<IReadOnlyList<BranchLookupDto>> GetBranchesAsync(
            CancellationToken cancellationToken);

        Task<IReadOnlyList<WarehouseLookupDto>> GetWarehousesAsync(
            CancellationToken cancellationToken);

        Task<PagedResult<OpenPoLineDto>> GetOpenPoLinesAsync(
            OpenPoQueryRequest request,
            CancellationToken cancellationToken);

        Task<ReceivingSaveResultDto> UpsertBatchAsync(
            UpsertReceivingRequest request,
            CancellationToken cancellationToken);

        Task<ReceivingSaveResultDto> ReverseAsync(
            string rrNo,
            ReverseReceivingRequest request,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<ReceivingItemLookupDto>> GetItemsAsync(
    string? search,
    string? whseCode,
    CancellationToken cancellationToken);
    }
}
