namespace twbERRANDSapi.AUTH
{
    public interface IAuthRepository
    {
        //Task<LoginUserDto?> LoginAsync(string userId, string password, CancellationToken cancellationToken = default);
        Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    }
}
