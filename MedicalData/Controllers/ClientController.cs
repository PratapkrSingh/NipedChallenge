using MedicalData.Models;
using MedicalData.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalData.Controllers
{
    [Authorize(Policy = "ClientEditorPolicy")]
    [ApiController]
    [Route("[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly IClientRepository _clientRepository;
        private readonly ILogger<ClientController> _logger;

        public ClientController(IClientRepository clientRepository, ILogger<ClientController> logger)
        {
            _clientRepository = clientRepository;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a list of all clients.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Client>), 200)]
        public async Task<ActionResult<IEnumerable<Client>>> GetClients()
        {
            var clients = await _clientRepository.GetAllClientsAsync();
            return Ok(clients);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Client), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Client>> GetClient(string id)
        {
            var client = await _clientRepository.GetClientByIdAsync(id);
            if (client == null)
            {
                _logger.LogWarning($"Client with ID {id} not found.");
                return NotFound($"Client with ID {id} not found.");
            }

            return Ok(client);
        }

        [HttpPost]
        [ProducesResponseType(typeof(Client), 201)] // 201 Created
        [ProducesResponseType(400)] // Bad Request
        public async Task<ActionResult<Client>> CreateClient([FromBody] Client client)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            client.Id = client.Id ?? System.Guid.NewGuid().ToString();

            if (await _clientRepository.GetClientByIdAsync(client.Id) != null)
            {
                return Conflict($"Client with ID {client.Id} already exists.");
            }

            await _clientRepository.AddClientAsync(client);
            _logger.LogInformation($"Client {client.Id} created successfully.");
            return CreatedAtAction(nameof(GetClient), new { id = client.Id }, client);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(204)] // No Content
        [ProducesResponseType(400)] // Bad Request
        [ProducesResponseType(404)] // Not Found
        public async Task<IActionResult> UpdateClient(string id, [FromBody] Client client)
        {
            if (id != client.Id)
            {
                return BadRequest("Client ID in URL does not match client ID in body.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingClient = await _clientRepository.GetClientByIdAsync(id);
            if (existingClient == null)
            {
                return NotFound($"Client with ID {id} not found.");
            }

            await _clientRepository.UpdateClientAsync(client);
            _logger.LogInformation($"Client {client.Id} updated successfully.");
            return NoContent();
        }

    }
}
