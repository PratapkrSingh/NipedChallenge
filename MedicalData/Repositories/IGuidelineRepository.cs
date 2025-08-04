using MedicalData.Models;

namespace MedicalData.Repositories
{
    public interface IGuidelineRepository
    {
        Task<MedicalGuidelines> GetMedicalGuidelinesAsync();
        Task SaveMedicalGuidelinesAsync(MedicalGuidelines guidelines);
    }
}
