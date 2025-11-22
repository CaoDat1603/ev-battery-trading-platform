namespace Catalog.Application.DTOs
{
    public class FeeSettingsDto
    {
        public int FeeId { get; set; }
        public int Type { get; set; }
        public decimal FeePercent { get; set; }
        public decimal CommissionPercent { get; set; }
        public DateTimeOffset? EffectiveDate { get; set; }
        public DateTimeOffset? EndedDate { get; set; }
        public bool IsActive { get; set; }
    }
}
