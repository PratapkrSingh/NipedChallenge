using MedicalData.Models;

namespace MedicalData.Repositories
{
    public interface IClientRepository
    {
        Task<IEnumerable<Client>> GetAllClientsAsync();
        Task<Client> GetClientByIdAsync(string id);
        Task AddClientAsync(Client client);
        Task UpdateClientAsync(Client client);
    }
}
