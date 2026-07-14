using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SAV.application.Repository;
using SAV.domain.Entities.Api;

namespace SAV.persistencia.Repositorios.Api;

public class ClientesUpdateApiRepo : IClientesUpdateApiRepo
{
    private readonly HttpClient _httpClient;
    private readonly string _url;
    private readonly ILogger<ClientesUpdateApiRepo> _logger;

    public ClientesUpdateApiRepo(HttpClient httpClient, IConfiguration config, ILogger<ClientesUpdateApiRepo> logger)
    {
        _httpClient = httpClient;
        _url = config["ExternalApi:ClientesUrl"] ?? "http://localhost:3000/api/ClientesApi/GetClientes";
        _logger = logger;
    }

    public async Task<IEnumerable<ClientesUpdate>> GetClientesUpdateAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(_url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ClientesUpdate>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<ClientesUpdate>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al obtener clientes desde API. Usando datos mock.");
            return GetMockClientes();
        }
    }

    private static IEnumerable<ClientesUpdate> GetMockClientes()
    {
        return new List<ClientesUpdate>
        {
            new() { CustomerID = 1, FirstName = "Juan", LastName = "Perez", Email = "juan@email.com", Phone = "123456789", City = "Santo Domingo", Country = "Republica Dominicana" },
            new() { CustomerID = 2, FirstName = "Maria", LastName = "Lopez", Email = "maria@email.com", Phone = "987654321", City = "Santiago", Country = "Republica Dominicana" }
        };
    }
}
