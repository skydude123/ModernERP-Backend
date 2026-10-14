using System.ComponentModel.DataAnnotations;

namespace HRIStwinbeeAPI.DTO.Inventory.ItemMaster
{
    public class ItemMasterUpsertRequestDto
    {
        [Required]
        [StringLength(
            10,
            ErrorMessage = "Item Code cannot exceed 10 characters."
        )]
        public string ItemCode { get; set; } = string.Empty;


        [Required]
        [StringLength(
            250,
            ErrorMessage = "Item Description cannot exceed 250 characters."
        )]
        public string ItemDesc { get; set; } = string.Empty;


        [Required]
        [StringLength(
            50,
            ErrorMessage = "Group Code cannot exceed 50 characters."
        )]
        public string GroupCode { get; set; } = string.Empty;


        [Required]
        [StringLength(
            10,
            ErrorMessage = "Unit of Measure cannot exceed 10 characters."
        )]
        public string Um { get; set; } = string.Empty;


        [StringLength(
            10,
            ErrorMessage = "User ID cannot exceed 10 characters."
        )]
        public string UserId { get; set; } = string.Empty;


        [StringLength(50)]
        public string? ItemType { get; set; }


        [Range(
            typeof(decimal),
            "0",
            "99999999999999"
        )]
        public decimal BufferQty { get; set; }


        [Range(
            typeof(decimal),
            "0",
            "99999999999999"
        )]
        public decimal UnitCost { get; set; }


        [Range(
            typeof(decimal),
            "0",
            "99999999999999"
        )]
        public decimal LastUnitCost { get; set; }


        [RegularExpression(
            "^[YN]$",
            ErrorMessage = "Active must be Y or N."
        )]
        public string Active { get; set; } = "Y";
    }
}