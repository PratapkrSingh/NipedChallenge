namespace MedicalData.Models
{
    public class Client
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public MedicalData MedicalData { get; set; }
    }
}
