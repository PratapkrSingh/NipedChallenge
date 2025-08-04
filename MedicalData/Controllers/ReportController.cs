using MedicalData.Domain;
using MedicalData.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace MedicalData.Controllers
{
    [Authorize(Policy = "ReportViewerPolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ReportController> _logger;

        private readonly string _clientServiceBaseUrl;
        private readonly string _guidelineServiceBaseUrl;

        public ReportController(IHttpClientFactory httpClientFactory,
                                Microsoft.Extensions.Configuration.IConfiguration configuration,
                                ILogger<ReportController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _clientServiceBaseUrl = configuration["ServiceUrls:ClientManagement"];
            _guidelineServiceBaseUrl = configuration["ServiceUrls:GuidelineManagement"];
        }

        /// <summary>
        /// Generates a health report for a specific client.
        /// </summary>
        /// <param name="clientId">The ID of the client for whom to generate the report.</param>
        [HttpGet("client/{clientId}")]
        [ProducesResponseType(typeof(ClientHealthReport), 200)]
        [ProducesResponseType(404)] // Client or Guidelines not found
        [ProducesResponseType(500)] // Internal Server Error (e.g., upstream service error)
        public async Task<ActionResult<ClientHealthReport>> GenerateClientReport(string clientId)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var clientResponse = await httpClient.GetAsync($"{_clientServiceBaseUrl}/api/client/{clientId}");

            if (clientResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"Client {clientId} not found for report generation.");
                return NotFound($"Client with ID {clientId} not found.");
            }
            clientResponse.EnsureSuccessStatusCode();
            var client = await JsonSerializer.DeserializeAsync<Client>(await clientResponse.Content.ReadAsStreamAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (client == null)
            {
                _logger.LogError($"Failed to deserialize client data for {clientId}.");
                return StatusCode(500, "Failed to process client data from upstream service.");
            }

            var guidelineResponse = await httpClient.GetAsync($"{_guidelineServiceBaseUrl}/api/guideline");
            guidelineResponse.EnsureSuccessStatusCode();
            var guidelines = await JsonSerializer.DeserializeAsync<MedicalGuidelines>(await guidelineResponse.Content.ReadAsStreamAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (guidelines == null)
            {
                _logger.LogError("Failed to deserialize medical guidelines.");
                return StatusCode(500, "Failed to retrieve medical guidelines from upstream service.");
            }

            var healthEvaluator = new HealthEvaluator(guidelines);

            var report = new ClientHealthReport
            {
                ClientId = client.Id,
                ClientName = client.Name,
                HealthMetrics = new Dictionary<string, MetricResult>(),
                QualitativeMetrics = new Dictionary<string, QualitativeResult>()
            };

            if (client.MedicalData?.Bloodwork?.Cholesterol != null)
            {
                report.HealthMetrics["CholesterolTotal"] = new MetricResult
                {
                    ClientValue = client.MedicalData.Bloodwork.Cholesterol.Total,
                    Status = healthEvaluator.EvaluateMetric(client.MedicalData.Bloodwork.Cholesterol.Total, guidelines.Cholesterol.Total),
                    GuidelineRange = guidelines.Cholesterol.Total.Optimal + "/" + guidelines.Cholesterol.Total.NeedsAttention + "/" + guidelines.Cholesterol.Total.SeriousIssue
                };
                report.HealthMetrics["CholesterolHdl"] = new MetricResult
                {
                    ClientValue = client.MedicalData.Bloodwork.Cholesterol.Hdl,
                    Status = healthEvaluator.EvaluateMetric(client.MedicalData.Bloodwork.Cholesterol.Hdl, guidelines.Cholesterol.Hdl),
                    GuidelineRange = guidelines.Cholesterol.Hdl.Optimal + "/" + guidelines.Cholesterol.Hdl.NeedsAttention + "/" + guidelines.Cholesterol.Hdl.SeriousIssue
                };
                report.HealthMetrics["CholesterolLdl"] = new MetricResult
                {
                    ClientValue = client.MedicalData.Bloodwork.Cholesterol.Ldl,
                    Status = healthEvaluator.EvaluateMetric(client.MedicalData.Bloodwork.Cholesterol.Ldl, guidelines.Cholesterol.Ldl),
                    GuidelineRange = guidelines.Cholesterol.Ldl.Optimal + "/" + guidelines.Cholesterol.Ldl.NeedsAttention + "/" + guidelines.Cholesterol.Ldl.SeriousIssue
                };
            }

            if (client.MedicalData?.Bloodwork != null)
            {
                report.HealthMetrics["BloodSugar"] = new MetricResult
                {
                    ClientValue = client.MedicalData.Bloodwork.BloodSugar,
                    Status = healthEvaluator.EvaluateMetric(client.MedicalData.Bloodwork.BloodSugar, guidelines.BloodSugar),
                    GuidelineRange = guidelines.BloodSugar.Optimal + "/" + guidelines.BloodSugar.NeedsAttention + "/" + guidelines.BloodSugar.SeriousIssue
                };
            }

            if (client.MedicalData?.Bloodwork?.BloodPressure != null)
            {
                report.HealthMetrics["BloodPressure"] = new MetricResult
                {
                    ClientValue = client.MedicalData.Bloodwork.BloodPressure.Systolic,
                    Status = healthEvaluator.EvaluateBloodPressure(
                        client.MedicalData.Bloodwork.BloodPressure.Systolic,
                        client.MedicalData.Bloodwork.BloodPressure.Diastolic,
                        guidelines.BloodPressure
                    ),

                    GuidelineRange = $"Optimal (S:{guidelines.BloodPressure.Optimal.Systolic}, D:{guidelines.BloodPressure.Optimal.Diastolic}) / " +
                                     $"Needs Attention (S:{guidelines.BloodPressure.NeedsAttention.Systolic}, D:{guidelines.BloodPressure.NeedsAttention.Diastolic}) / " +
                                     $"Serious Issue (S:{guidelines.BloodPressure.SeriousIssue.Systolic}, D:{guidelines.BloodPressure.SeriousIssue.Diastolic})"
                };
            }

            if (client.MedicalData?.Questionnaire != null)
            {
                report.QualitativeMetrics["ExerciseWeeklyMinutes"] = new QualitativeResult
                {
                    ClientValue = client.MedicalData.Questionnaire.ExerciseWeeklyMinutes.ToString(), // Convert int to string for qualitative result
                    Status = healthEvaluator.EvaluateMetric(client.MedicalData.Questionnaire.ExerciseWeeklyMinutes, guidelines.ExerciseWeeklyMinutes),
                    GuidelineRange = guidelines.ExerciseWeeklyMinutes.Optimal + "/" + guidelines.ExerciseWeeklyMinutes.NeedsAttention + "/" + guidelines.ExerciseWeeklyMinutes.SeriousIssue
                };
                report.QualitativeMetrics["SleepQuality"] = new QualitativeResult
                {
                    ClientValue = client.MedicalData.Questionnaire.SleepQuality,
                    Status = healthEvaluator.EvaluateQualitative(client.MedicalData.Questionnaire.SleepQuality, guidelines.SleepQuality),
                    GuidelineRange = guidelines.SleepQuality.Optimal + "/" + guidelines.SleepQuality.NeedsAttention + "/" + guidelines.SleepQuality.SeriousIssue
                };
                report.QualitativeMetrics["StressLevels"] = new QualitativeResult
                {
                    ClientValue = client.MedicalData.Questionnaire.StressLevels,
                    Status = healthEvaluator.EvaluateQualitative(client.MedicalData.Questionnaire.StressLevels, guidelines.StressLevels),
                    GuidelineRange = guidelines.StressLevels.Optimal + "/" + guidelines.StressLevels.NeedsAttention + "/" + guidelines.StressLevels.SeriousIssue
                };
                report.QualitativeMetrics["DietQuality"] = new QualitativeResult
                {
                    ClientValue = client.MedicalData.Questionnaire.DietQuality,
                    Status = healthEvaluator.EvaluateQualitative(client.MedicalData.Questionnaire.DietQuality, guidelines.DietQuality),
                    GuidelineRange = guidelines.DietQuality.Optimal + "/" + guidelines.DietQuality.NeedsAttention + "/" + guidelines.DietQuality.SeriousIssue
                };
            }

            _logger.LogInformation($"Health report generated for client {clientId}.");
            return Ok(report);
        }
    }
}
