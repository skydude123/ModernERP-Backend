using System.ComponentModel.DataAnnotations;

namespace HRIStwinbeeAPI.DTO.AssetAcceptance;

public static class AssetAccountPurposes
{
    public const string AssetCost =
        "ASSET_COST";

    public const string DepreciationExpense =
        "DEPRECIATION_EXPENSE";

    public const string AccumulatedDepreciation =
        "ACCUMULATED_DEPRECIATION";

    public static string Normalize(
        string? purpose)
    {
        var normalized =
            purpose?
                .Trim()
                .ToUpperInvariant()
                .Replace(" ", "_")
                .Replace("-", "_");

        return normalized switch
        {
            AssetCost =>
                AssetCost,

            DepreciationExpense =>
                DepreciationExpense,

            AccumulatedDepreciation =>
                AccumulatedDepreciation,

            _ => throw new ValidationException(
                "Purpose must be ASSET_COST, " +
                "DEPRECIATION_EXPENSE, or " +
                "ACCUMULATED_DEPRECIATION.")
        };
    }
}