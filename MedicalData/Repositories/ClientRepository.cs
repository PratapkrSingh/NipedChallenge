using MedicalData.Models;
using System.Text.Json;

namespace MedicalData.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private List<Client> _clients;
        private readonly string _clientDataFilePath = "Data/clientData.json"; // Adjust path as necessary

        public ClientRepository()
        {
            LoadInitialClientData();
        }

        private void LoadInitialClientData()
        {
            if (File.Exists(_clientDataFilePath))
            {
                var jsonString = File.ReadAllText(_clientDataFilePath);
                var clientDataWrapper = JsonSerializer.Deserialize<ClientDataWrapper>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                _clients = clientDataWrapper?.Clients ?? new List<Client>();
            }
            else
            {
                _clients = new List<Client>();
            }
        }

        private class ClientDataWrapper
        {
            public List<Client> Clients { get; set; }
        }

        public Task<IEnumerable<Client>> GetAllClientsAsync()
        {
            return Task.FromResult<IEnumerable<Client>>(_clients);
        }

        public Task<Client> GetClientByIdAsync(string id)
        {
            return Task.FromResult(_clients.FirstOrDefault(c => c.Id == id));
        }

        public Task AddClientAsync(Client client)
        {
            _clients.Add(client);
            // In a real app, save to DB here
            return Task.CompletedTask;
        }

        public Task UpdateClientAsync(Client client)
        {
            var existingClient = _clients.FirstOrDefault(c => c.Id == client.Id);
            if (existingClient != null)
            {
                _clients.Remove(existingClient);
                _clients.Add(client);
                // In a real app, update in DB here
            }
            return Task.CompletedTask;
        }
    }
}
