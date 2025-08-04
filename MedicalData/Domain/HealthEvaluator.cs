using MedicalData.Models;

namespace MedicalData.Domain
{
    public class HealthEvaluator
    {
        private readonly MedicalGuidelines _guidelines;

        public HealthEvaluator(MedicalGuidelines guidelines)
        {
            _guidelines = guidelines ?? throw new ArgumentNullException(nameof(guidelines), "Medical guidelines cannot be null.");
        }

        public string EvaluateMetric(double clientValue, MetricGuideline guideline)
        {
            if (guideline == null) return "N/A - Guideline Missing";

            if (ParseAndCheckRange(clientValue, guideline.Optimal)) return "Optimal";
            if (ParseAndCheckRange(clientValue, guideline.NeedsAttention)) return "Needs Attention";
            if (ParseAndCheckRange(clientValue, guideline.SeriousIssue)) return "Serious Issue";

            return "N/A - Value Out of Defined Ranges";
        }

        public string EvaluateBloodPressure(int systolic, int diastolic, BloodPressureGuideline guideline)
        {
            if (guideline == null) return "N/A - Guideline Missing";

            if (ParseAndCheckBpRange(systolic, diastolic, guideline.SeriousIssue)) return "Serious Issue";
            if (ParseAndCheckBpRange(systolic, diastolic, guideline.NeedsAttention)) return "Needs Attention";
            if (ParseAndCheckBpRange(systolic, diastolic, guideline.Optimal)) return "Optimal";

            return "N/A - BP Out of Defined Ranges";
        }

        public string EvaluateQualitative(string clientValue, QualitativeGuideline guideline)
        {
            if (guideline == null) return "N/A - Guideline Missing";
            if (string.IsNullOrWhiteSpace(clientValue)) return "N/A - Client Value Missing";

            if (guideline.Optimal != null && clientValue.Contains(GetKeyword(guideline.Optimal), StringComparison.OrdinalIgnoreCase)) return "Optimal";
            if (guideline.NeedsAttention != null && clientValue.Contains(GetKeyword(guideline.NeedsAttention), StringComparison.OrdinalIgnoreCase)) return "Needs Attention";
            if (guideline.SeriousIssue != null && clientValue.Contains(GetKeyword(guideline.SeriousIssue), StringComparison.OrdinalIgnoreCase)) return "Serious Issue";

            return "N/A - Qualitative Value Uncategorized";
        }

        private bool ParseAndCheckRange(double value, string rangeString)
        {
            if (string.IsNullOrWhiteSpace(rangeString)) return false;

            rangeString = rangeString.Trim();

            if (rangeString.StartsWith("<="))
            {
                if (double.TryParse(rangeString.AsSpan(2), out var limit)) return value <= limit;
            }
            else if (rangeString.StartsWith("<"))
            {
                if (double.TryParse(rangeString.AsSpan(1), out var limit)) return value < limit;
            }
            else if (rangeString.StartsWith(">="))
            {
                if (double.TryParse(rangeString.AsSpan(2), out var limit)) return value >= limit;
            }
            else if (rangeString.StartsWith(">"))
            {
                if (double.TryParse(rangeString.AsSpan(1), out var limit)) return value > limit;
            }
            else if (rangeString.Contains("-"))
            {
                var parts = rangeString.Split('-');
                if (parts.Length == 2 && double.TryParse(parts[0], out double min) && double.TryParse(parts[1], out double max))
                {
                    return value >= min && value <= max;
                }
            }
            else if (double.TryParse(rangeString, out var exactValue))
            {
                return value == exactValue;
            }
            return false;
        }

        private bool ParseAndCheckBpRange(int systolic, int diastolic, BloodPressureRange bpRange)
        {
            if (string.IsNullOrWhiteSpace(bpRange.Systolic) || string.IsNullOrWhiteSpace(bpRange.Diastolic)) return false;

            var systolicMatch = ParseAndCheckRange(systolic, bpRange.Systolic);
            var diastolicMatch = ParseAndCheckRange(diastolic, bpRange.Diastolic);

            // For blood pressure & based on the provided guidelines:
            // Optimal: S <120 AND D <80
            if (bpRange == _guidelines.BloodPressure.Optimal)
            {
                return systolicMatch && diastolicMatch;
            }

            // Needs Attention: S 120-129 AND D <80
            if (bpRange == _guidelines.BloodPressure.NeedsAttention)
            {
                return systolicMatch && diastolicMatch;
            }

            // Serious Issue: S >=130 OR D >=80
            if (bpRange == _guidelines.BloodPressure.SeriousIssue)
            {
                return systolicMatch || diastolicMatch; // OR condition for serious
            }

            return false;
        }

        private static string GetKeyword(string description)
        {
            if (string.IsNullOrWhiteSpace(description)) return string.Empty;

            if (description.Contains("restful sleep", StringComparison.OrdinalIgnoreCase)) return "restful sleep";
            if (description.Contains("frequent disturbances", StringComparison.OrdinalIgnoreCase)) return "frequent disturbances";
            if (description.Contains("severe sleep issues", StringComparison.OrdinalIgnoreCase)) return "severe sleep issues";
            if (description.Contains("Low self-reported stress", StringComparison.OrdinalIgnoreCase)) return "Low self-reported stress";
            if (description.Contains("Moderate self-reported stress", StringComparison.OrdinalIgnoreCase)) return "Moderate self-reported stress";
            if (description.Contains("High chronic stress", StringComparison.OrdinalIgnoreCase)) return "High chronic stress";
            if (description.Contains("Balanced, nutrient-rich diet", StringComparison.OrdinalIgnoreCase)) return "Balanced, nutrient-rich diet";
            if (description.Contains("Processed or high-sugar diet", StringComparison.OrdinalIgnoreCase)) return "Processed or high-sugar diet";
            return description.Contains("Poor nutrition with deficiencies", StringComparison.OrdinalIgnoreCase) ? "Poor nutrition with deficiencies" : string.Join(" ", description.Split(' ').Take(3));
        }
    }
}
