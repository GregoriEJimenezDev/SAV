using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SAV.application.Repository;
using SAV.domain.Entities.Api;

namespace SAV.persistencia.Repositorios.Api;

public class ProductosUpdateApiRepo : IProductosUpdateApiRepo
{
    private readonly HttpClient _httpClient;
    private readonly string _url;
    private readonly ILogger<ProductosUpdateApiRepo> _logger;

    public ProductosUpdateApiRepo(HttpClient httpClient, IConfiguration config, ILogger<ProductosUpdateApiRepo> logger)
    {
        _httpClient = httpClient;
        _url = config["ExternalApi:ProductosUrl"] ?? "http://localhost:3000/api/ProductosApi/GetProductos";
        _logger = logger;
    }

    public async Task<IEnumerable<ProductosUpdate>> GetProductosUpdateAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(_url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ProductosUpdate>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<ProductosUpdate>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al obtener productos desde API. Usando datos mock.");
            return GetMockProductos();
        }
    }

    private static IEnumerable<ProductosUpdate> GetMockProductos()
    {
        return new List<ProductosUpdate>
        {
            new() { ProductID = 1, ProductName = "Cafe Premium", Category = "Bebidas", Price = 250.00m, Stock = 100 },
            new() { ProductID = 2, ProductName = "Arroz Organico", Category = "Alimentos", Price = 85.50m, Stock = 200 },
            new() { ProductID = 3, ProductName = "Frijoles Negros", Category = "Alimentos", Price = 95.00m, Stock = 150 },
            new() { ProductID = 4, ProductName = "Aceite de Oliva", Category = "Condimentos", Price = 320.00m, Stock = 80 },
            new() { ProductID = 5, ProductName = "Pan Integral", Category = "Panaderia", Price = 60.00m, Stock = 300 }
        };
    }
}
