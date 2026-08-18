using System;

namespace PharmacyInventory.Domain.Entities
{
    /// <summary>
    /// Core domain entity. No dependency on any other layer (Clean Architecture rule:
    /// Domain is the innermost circle and knows nothing about Application/Infrastructure/API).
    /// </summary>
    public class Medicine
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Brand { get; set; } = string.Empty;
    }
}
