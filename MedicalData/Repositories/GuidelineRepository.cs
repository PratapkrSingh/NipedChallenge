using MedicalData.Models;
using System.Text.Json;

namespace MedicalData.Repositories
{
    public class GuidelineRepository : IGuidelineRepository
    {
        private MedicalGuidelines _guidelines;
        private readonly string _guidelinesFilePath = "Data/medicalGuidelines.json";

        public GuidelineRepository()
        {
            LoadInitialGuidelines();
        }

        private void LoadInitialGuidelines()
        {
            if (File.Exists(_guidelinesFilePath))
            {
                var jsonString = File.ReadAllText(_guidelinesFilePath);
                var guidelineWrapper = JsonSerializer.Deserialize<GuidelineWrapper>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                _guidelines = guidelineWrapper?.Guidelines;
            }
            else
            {
                _guidelines = new MedicalGuidelines();
            }
        }

        private class GuidelineWrapper
        {
            public MedicalGuidelines Guidelines { get; set; }
        }

        public Task<MedicalGuidelines> GetMedicalGuidelinesAsync()
        {
            return Task.FromResult(_guidelines);
        }

        public Task SaveMedicalGuidelinesAsync(MedicalGuidelines guidelines)
        {
            _guidelines = guidelines;
            return Task.CompletedTask;
        }
    }
}
