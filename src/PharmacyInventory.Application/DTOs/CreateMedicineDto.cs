using System;

namespace PharmacyInventory.Application.DTOs
{
    /// <summary>
    /// Plain data holder. Validation rules live in <see cref="Validators.CreateMedicineDtoValidator"/>
    /// so there's a single, testable source of truth instead of splitting rules between
    /// Data Annotations and FluentValidation.
    /// </summary>
    public class CreateMedicineDto
    {
        public string FullName { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Brand { get; set; } = string.Empty;
    }
}
