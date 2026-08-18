namespace PharmacyInventory.Application.Common
{
    /// <summary>
    /// Central place for the thresholds used for grid color-coding, so they are
    /// defined once and easy to change without hunting through the codebase.
    /// </summary>
    public static class BusinessRules
    {
        public const int ExpiryWarningDays = 30;
        public const int LowStockThreshold = 10;
    }
}
