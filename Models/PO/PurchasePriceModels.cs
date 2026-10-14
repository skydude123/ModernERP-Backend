namespace HRIStwinbeeAPI.Models.PO;

public static class PurchasePriceFieldLengths
{
    public const int SupplierCode = 10;
    public const int ItemCode = 20;
    public const int PoUm = 3;
    public const int Remarks = 500;
    public const int SourceReference = 50;
}

public sealed class PurchasePricePagedResult<T>
{
    public IReadOnlyList<T> Data { get; set; } = Array.Empty<T>();
    public long Total { get; set; }
}

public sealed class PurchasePriceItemUm
{
    public string ItemCode { get; set; } = string.Empty;
    public string PoUm { get; set; } = string.Empty;
    public string UmDescription { get; set; } = string.Empty;
    public decimal ConversionFactor { get; set; }
    public string BaseUm { get; set; } = string.Empty;
}

public sealed class SupplierPriceListItem
{
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public string PoUm { get; set; } = string.Empty;

    public string? BaseUm { get; set; }

    public decimal? PoConvFactor { get; set; }

    public decimal? UnitPrice { get; set; }

    public DateTime? EffectiveDate { get; set; }

    public DateTime? DateChanged { get; set; }

    public bool? IsActive { get; set; }
}

public class SupplierPriceRecord
{
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public string PoUm { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public DateTime EffectiveDate { get; set; }
    public bool IsActive { get; set; }
    public string? Remarks { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime ModifiedAt { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

public sealed class SupplierPriceUpsertResult : SupplierPriceRecord
{
    public bool WasChanged { get; set; }
}

public sealed class SupplierPriceUpsertRequest
{
    public string SupplierCode { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string PoUm { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public DateTime EffectiveDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Remarks { get; set; }
    public string? SourceReference { get; set; }
    public byte[]? ExpectedRowVersion { get; set; }
}

public sealed class PurchasePriceResolution
{
    public string SupplierCode { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public string PoUm { get; set; } = string.Empty;
    public decimal? PoConvFactor { get; set; }
    public string? BaseUm { get; set; }
    public bool IsUmValid { get; set; }
    public decimal? SuggestedPrice { get; set; }
    public string SuggestedSource { get; set; } = "NONE";
    public decimal? CurrentSupplierMasterPrice { get; set; }
    public DateTime? CurrentSupplierEffectiveDate { get; set; }
    public byte[]? CurrentSupplierRowVersion { get; set; }
    public decimal? ConfiguredSupplierMasterPrice { get; set; }
    public DateTime? ConfiguredSupplierEffectiveDate { get; set; }
    public bool? ConfiguredSupplierIsActive { get; set; }
    public string? ConfiguredSupplierRemarks { get; set; }
    public byte[]? ConfiguredSupplierRowVersion { get; set; }
    public decimal? LastActualPoPrice { get; set; }
    public string? LastActualPoNo { get; set; }
    public DateTime? LastActualPoDate { get; set; }
    public decimal? LegacyPrice { get; set; }
    public decimal? LegacyConvertedPrice { get; set; }
    public bool IsLegacyUmCompatible { get; set; }
}

public sealed class SupplierPriceComparison
{
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string PoUm { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsSelectedSupplier { get; set; }
}

public sealed class PurchasePriceInquiryAggregate
{
    public long Total { get; set; }
    public decimal? LowestConfiguredPrice { get; set; }
    public decimal? HighestConfiguredPrice { get; set; }
}

public sealed class PurchasePriceInquiryResult
{
    public PurchasePriceResolution Resolution { get; set; } = new();
    public IReadOnlyList<SupplierPriceComparison> ConfiguredPrices { get; set; } =
        Array.Empty<SupplierPriceComparison>();
    public long Total { get; set; }
    public decimal? LowestConfiguredPrice { get; set; }
    public decimal? HighestConfiguredPrice { get; set; }
}

public sealed class SupplierPriceHistoryRecord
{
    public long HistoryId { get; set; }
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string PoUm { get; set; } = string.Empty;
    public decimal? OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public string? SourceReference { get; set; }
    public string? Remarks { get; set; }
    public bool? OldIsActive { get; set; }
    public bool NewIsActive { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
}

public sealed class ActualPoPriceHistoryRecord
{
    public string PoNo { get; set; } = string.Empty;
    public int PoLineNo { get; set; }
    public DateTime PoDate { get; set; }
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public string PoUm { get; set; } = string.Empty;
    public decimal PoConvFactor { get; set; }
    public decimal QtyPo { get; set; }
    public decimal UnitPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
}

public sealed class PurchasePriceCurrentQuery
{
    public string? SupplierCode { get; set; }
    public string? ItemCode { get; set; }
    public string? PoUm { get; set; }
    public string Active { get; set; } = "ALL";
    public DateTime? AsOfDate { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; } = 50;
}

public sealed class PurchasePriceHistoryQuery
{
    public string? SupplierCode { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string PoUm { get; set; } = string.Empty;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; } = 50;
}

public abstract class PurchasePriceException(
    string title,
    string message,
    int statusCode,
    Exception? innerException = null)
    : Exception(message, innerException)
{
    public string Title { get; } = title;
    public int StatusCode { get; } = statusCode;
}

public sealed class PurchasePriceValidationException(
    string message,
    Exception? innerException = null)
    : PurchasePriceException("Purchase price validation failed.", message, 400, innerException);

public sealed class PurchasePriceConflictException(
    string message,
    Exception? innerException = null)
    : PurchasePriceException("Purchase price update conflicted with current data.", message, 409, innerException);
