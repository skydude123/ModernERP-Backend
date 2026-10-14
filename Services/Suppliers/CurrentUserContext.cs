using System.Security.Claims;
using HRIStwinbeeAPI.DTO.Suppliers;

namespace HRIStwinbeeAPI.Services.Suppliers;

public interface ICurrentUserContext
{
    string GetRequiredUserId();
}

public sealed class HttpCurrentUserContext(
    IHttpContextAccessor httpContextAccessor)
    : ICurrentUserContext
{
    public string GetRequiredUserId()
    {
        var principal = httpContextAccessor.HttpContext?.User;

        if (principal?.Identity?.IsAuthenticated != true)
        {
            throw new SupplierUnauthorizedException(
                "The authenticated user could not be identified.");
        }

        var userId =
            principal.FindFirstValue(ClaimTypes.NameIdentifier) ??
            principal.FindFirstValue("user_id") ??
            principal.FindFirstValue("sub");

        userId = userId?.Trim();

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new SupplierUnauthorizedException(
                "The authenticated user could not be identified.");
        }

        if (userId.Length > SupplierFieldLengths.UserId)
        {
            throw new SupplierUnauthorizedException(
                "The authenticated user identifier exceeds the supplier audit-field limit.");
        }

        return userId;
    }
}
