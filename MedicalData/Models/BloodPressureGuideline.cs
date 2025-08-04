namespace MedicalData.Models
{
    public class BloodPressureGuideline
    {
        public BloodPressureRange Optimal { get; set; }
        public BloodPressureRange NeedsAttention { get; set; }
        public BloodPressureRange SeriousIssue { get; set; }
    }
}
