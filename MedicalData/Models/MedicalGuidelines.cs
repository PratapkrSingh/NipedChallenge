namespace MedicalData.Models
{
    public class MedicalGuidelines
    {
        public GuidelineSet Cholesterol { get; set; }
        public MetricGuideline BloodSugar { get; set; }
        public BloodPressureGuideline BloodPressure { get; set; }
        public MetricGuideline ExerciseWeeklyMinutes { get; set; }
        public QualitativeGuideline SleepQuality { get; set; }
        public QualitativeGuideline StressLevels { get; set; }
        public QualitativeGuideline DietQuality { get; set; }
    }
}
