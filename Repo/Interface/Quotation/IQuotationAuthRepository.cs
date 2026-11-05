
using System.Threading;
using System.Threading.Tasks;

using twbERRANDSapi.AUTH;

namespace HRIStwinbeeAPI.Repo.Interface.Quotation
{
    public interface IQuotationAuthRepository
    {
        Task<LoginResponse> LoginAsync(
            LoginRequest request,
            CancellationToken cancellationToken = default);
    }
}
