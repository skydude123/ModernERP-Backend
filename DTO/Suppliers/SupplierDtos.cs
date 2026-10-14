using System.ComponentModel.DataAnnotations;

namespace HRIStwinbeeAPI.DTO.Suppliers;

public static class SupplierFieldLengths
{
    public const int Code = 10;
    public const int Name = 100;
    public const int GroupCode = 5;
    public const int AddressLine1 = 200;
    public const int AddressLine2 = 35;
    public const int AddressLine3 = 35;
    public const int ContactPerson = 35;
    public const int PositionTitle = 25;
    public const int Telephone = 30;
    public const int Fax = 30;
    public const int Email = 45;
    public const int TinNo = 50;
    public const int TermsCode = 4;
    public const int CurrencyCode = 5;
    public const int Website = 80;
    public const int Specialties = 500;
    public const int UserId = 10;
    public const int TaxProfileCode = 15;
}

public sealed class SupplierListQueryRequest
{
    [StringLength(100)]
    public string? Search { get; set; }

    [StringLength(SupplierFieldLengths.GroupCode)]
    public string? SupplierGroupCode { get; set; }

    [StringLength(SupplierFieldLengths.TermsCode)]
    public string? TermsCode { get; set; }

    [StringLength(SupplierFieldLengths.CurrencyCode)]
    public string? CurrencyCode { get; set; }

    [StringLength(SupplierFieldLengths.TaxProfileCode)]
    public string? TaxProfileCode { get; set; }

    [RegularExpression("^(ALL|Y|N)$", ErrorMessage = "ActiveFlag must be ALL, Y, or N.")]
    public string ActiveFlag { get; set; } = "Y";

    [Range(0, int.MaxValue)]
    public int Skip { get; set; }

    [Range(1, 250)]
    public int Take { get; set; } = 50;

    [StringLength(40)]
    public string SortField { get; set; } = "supplierName";

    [RegularExpression("^(?i:asc|desc)$", ErrorMessage = "SortDirection must be asc or desc.")]
    public string SortDirection { get; set; } = "asc";
}

public sealed class SupplierListItemDto
{
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string SupplierGroupCode { get; set; } = string.Empty;
    public string SupplierGroupName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Telephone { get; set; }
    public string? Email { get; set; }
    public string? TinNo { get; set; }
    public string? TermsCode { get; set; }
    public string? TermsDescription { get; set; }
    public string? CurrencyCode { get; set; }
    public string? CurrencyDescription { get; set; }
    public string? TaxProfileCode { get; set; }
    public string? TaxProfileDescription { get; set; }
    public string ActiveFlag { get; set; } = "Y";
    public string ModifiedBy { get; set; } = string.Empty;
    //public DateTime ModifiedDate { get; set; }
    public long TotalCount { get; set; }
}

public class SupplierDetailDto
{
    public string SupplierCode { get; set; } = string.Empty;

    public string SupplierName { get; set; } = string.Empty;

    public string SupplierGroupCode { get; set; } = string.Empty;

    public string SupplierGroupName { get; set; } = string.Empty;

    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public string? AddressLine3 { get; set; }

    public string? ContactPerson { get; set; }

    public string? PositionTitle { get; set; }

    public string? Telephone { get; set; }

    public string? Fax { get; set; }

    public string? Email { get; set; }

    public string? TinNo { get; set; }

    public string? TermsCode { get; set; }

    public string? TermsDescription { get; set; }

    public string? CurrencyCode { get; set; }

    public string? CurrencyDescription { get; set; }

    public string? Website { get; set; }

    public string? Specialties { get; set; }

    public string? TaxProfileCode { get; set; }

    public string? TaxProfileDescription { get; set; }

    public string ActiveFlag { get; set; } = "Y";

    public string ModifiedBy { get; set; } = string.Empty;

    public DateTime? ModifiedDate { get; set; }
}

//public class SupplierDetailDto
//{
//    public string SupplierCode { get; set; } = string.Empty;
//    public string SupplierName { get; set; } = string.Empty;
//    public string SupplierGroupCode { get; set; } = string.Empty;
//    public string SupplierGroupName { get; set; } = string.Empty;
//    public string? AddressLine1 { get; set; }
//    public string? AddressLine2 { get; set; }
//    public string? AddressLine3 { get; set; }
//    public string? ContactPerson { get; set; }
//    public string? PositionTitle { get; set; }
//    public string? Telephone { get; set; }
//    public string? Fax { get; set; }
//    public string? Email { get; set; }
//    public string? TinNo { get; set; }
//    public string? TermsCode { get; set; }
//    public string? TermsDescription { get; set; }
//    public string? CurrencyCode { get; set; }
//    public string? CurrencyDescription { get; set; }
//    public string? Website { get; set; }
//    public string? Specialties { get; set; }
//    public string? TaxProfileCode { get; set; }
//    public string? TaxProfileDescription { get; set; }
//    public string ActiveFlag { get; set; } = "Y";
//    public string ModifiedBy { get; set; } = string.Empty;
//    //public DateTime ModifiedDate { get; set; }
//}

public abstract class SupplierWriteRequest
{
    [Required, StringLength(SupplierFieldLengths.Name)]
    public string SupplierName { get; set; } = string.Empty;

    [Required, StringLength(SupplierFieldLengths.GroupCode)]
    public string SupplierGroupCode { get; set; } = string.Empty;

    [StringLength(SupplierFieldLengths.AddressLine1)]
    public string? AddressLine1 { get; set; }

    [StringLength(SupplierFieldLengths.AddressLine2)]
    public string? AddressLine2 { get; set; }

    [StringLength(SupplierFieldLengths.AddressLine3)]
    public string? AddressLine3 { get; set; }

    [StringLength(SupplierFieldLengths.ContactPerson)]
    public string? ContactPerson { get; set; }

    [StringLength(SupplierFieldLengths.PositionTitle)]
    public string? PositionTitle { get; set; }

    [StringLength(SupplierFieldLengths.Telephone)]
    public string? Telephone { get; set; }

    [StringLength(SupplierFieldLengths.Fax)]
    public string? Fax { get; set; }

    [EmailAddress, StringLength(SupplierFieldLengths.Email)]
    public string? Email { get; set; }

    [StringLength(SupplierFieldLengths.TinNo)]
    public string? TinNo { get; set; }

    [StringLength(SupplierFieldLengths.TermsCode)]
    public string? TermsCode { get; set; }

    [StringLength(SupplierFieldLengths.CurrencyCode)]
    public string? CurrencyCode { get; set; }

    [StringLength(SupplierFieldLengths.Website)]
    public string? Website { get; set; }

    [StringLength(SupplierFieldLengths.Specialties)]
    public string? Specialties { get; set; }

    [StringLength(SupplierFieldLengths.TaxProfileCode)]
    public string? TaxProfileCode { get; set; }

    public string? UserId { get; set; }
}

public sealed class SupplierCreateRequest : SupplierWriteRequest
{
    [Required, StringLength(SupplierFieldLengths.Code)]
    public string SupplierCode { get; set; } = string.Empty;
}

public sealed class SupplierUpdateRequest : SupplierWriteRequest
{
    [Required, StringLength(SupplierFieldLengths.Code)]
    public string SupplierCode { get; set; } = string.Empty;

    //[Required]
    //public DateTime? ExpectedModifiedDate { get; set; }
}

//public sealed class SupplierStatusRequest
//{
//    [Required]
//    public DateTime? ExpectedModifiedDate { get; set; }
//}

public sealed class SupplierLookupDto
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public sealed class SupplierPagedResult<T>
{
    public IReadOnlyList<T> Data { get; set; } = [];
    public long Total { get; set; }
}

public sealed class SupplierCommandResultDto
{
    public SupplierDetailDto Supplier { get; set; } = new();
    public string Message { get; set; } = string.Empty;
    public string? WarningMessage { get; set; }
}

public sealed class SupplierValidationResultDto
{
    public bool SupplierExists { get; set; }
    public bool SupplierCodeExists { get; set; }
    public bool SupplierGroupExists { get; set; }
    public bool TermsExists { get; set; }
    public bool CurrencyExists { get; set; }
    public bool TaxProfileExists { get; set; }
    public bool DuplicateTinExists { get; set; }
    public string? SimilarSupplierCode { get; set; }
    public string? SimilarSupplierName { get; set; }
    public string? WarningMessage { get; set; }
}

public sealed class SupplierCommandRow : SupplierDetailDto
{
    public string Message { get; set; } = string.Empty;
    public string? WarningMessage { get; set; }
}
