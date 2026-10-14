namespace HRIStwinbeeAPI.DTO.Suppliers;

public abstract class SupplierException(
    string title,
    string message,
    int statusCode,
    Exception? innerException = null)
    : Exception(message, innerException)
{
    public string Title { get; } = title;
    public int StatusCode { get; } = statusCode;
}

public sealed class SupplierValidationException(
    string message,
    Exception? innerException = null)
    : SupplierException(
        "Supplier validation failed.",
        message,
        400,
        innerException);

public sealed class SupplierUnauthorizedException(string message)
    : SupplierException(
        "Authentication is required.",
        message,
        401);

public sealed class SupplierNotFoundException(
    string message,
    Exception? innerException = null)
    : SupplierException(
        "Supplier was not found.",
        message,
        404,
        innerException);

public sealed class SupplierConflictException(
    string message,
    Exception? innerException = null)
    : SupplierException(
        "Supplier operation conflicted with current data.",
        message,
        409,
        innerException);
