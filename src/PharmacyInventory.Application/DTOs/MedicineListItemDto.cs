using System;

namespace PharmacyInventory.Application.DTOs
{
    /// <summary>
    /// Shape returned to the grid. Notes is intentionally excluded per the requirement
    /// ("results showing the medicine attributes (except Notes) should be displayed in a grid").
    /// IsExpiringSoon / IsLowStock are computed server-side so the frontend only needs to
    /// bind CSS classes, not re-implement business rules.
    /// </summary>
    public class MedicineListItemDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Brand { get; set; } = string.Empty;

        /// <summary>True when ExpiryDate is less than 30 days away -> red background.</summary>
        public bool IsExpiringSoon { get; set; }

        /// <summary>True when Quantity is less than 10 -> yellow background.</summary>
        public bool IsLowStock { get; set; }
    }
}
