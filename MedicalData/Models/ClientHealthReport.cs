namespace MedicalData.Models
{
    public class ClientHealthReport
    {
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public Dictionary<string, MetricResult> HealthMetrics { get; set; }
        public Dictionary<string, QualitativeResult> QualitativeMetrics { get; set; }
        public string OverallAssessment { get; set; }
    }
}
