using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SAV.application.Repository;
using SAV.application.Resultado;
using SAV.domain.Entities.Api;
using SAV.domain.Entities.Csv;
using SAV.domain.Entities.DB_Externa;
using SAV.domain.Entities.Data_Warehouse.Dimensions;
using SAV.domain.Entities.Data_Warehouse.Facts;

namespace SAV.persistencia.Repositorios.Data_Warehouse;

public class DimensionLoader : IDimensionLoader
{
    private readonly DwContext _context;
    private readonly string _connectionString;
    private readonly IproductsCsvRepo _productsRepo;
    private readonly IcustomersCsvRepo _customersRepo;
    private readonly IordersCsvRepo _ordersRepo;
    private readonly IClientesUpdateApiRepo _clientesApiRepo;
    private readonly IProductosUpdateApiRepo _productosApiRepo;
    private readonly IVentasHistoricasDBRepo _ventasHistoricasRepo;
    private readonly IVendedorCsvRepo _vendedorRepo;
    private readonly IConfiguration _config;
    private readonly ILogger<DimensionLoader> _logger;

    public DimensionLoader(
        DwContext context,
        IConfiguration config,
        IproductsCsvRepo productsRepo,
        IcustomersCsvRepo customersRepo,
        IordersCsvRepo ordersRepo,
        IClientesUpdateApiRepo clientesApiRepo,
        IProductosUpdateApiRepo productosApiRepo,
        IVentasHistoricasDBRepo ventasHistoricasRepo,
        IVendedorCsvRepo vendedorRepo,
        ILogger<DimensionLoader> logger)
    {
        _context = context;
        _connectionString = config.GetConnectionString("DtwarehouseConnString")
            ?? throw new InvalidOperationException("DtwarehouseConnString no configurado.");
        _productsRepo = productsRepo;
        _customersRepo = customersRepo;
        _ordersRepo = ordersRepo;
        _clientesApiRepo = clientesApiRepo;
        _productosApiRepo = productosApiRepo;
        _ventasHistoricasRepo = ventasHistoricasRepo;
        _vendedorRepo = vendedorRepo;
        _config = config;
        _logger = logger;
    }

    public async Task<Result> LoadDimsDataAsync()
    {
        try
        {
            _logger.LogInformation("Limpiando tablas de dimensiones...");
            await CleanDimensionTablesAsync();

            _logger.LogInformation("Cargando dimensiones desde CSV, API y BD externa...");
            var ahora = DateTime.Now;

            var fuentes = await CrearFuentesAsync(ahora);
            var csvProducts = await _productsRepo.ReadFileAsync(_config["Csv:products"]!);
            var csvCustomers = await _customersRepo.ReadFileAsync(_config["Csv:customers"]!);
            var apiClientes = await _clientesApiRepo.GetClientesUpdateAsync();
            var apiProductos = await _productosApiRepo.GetProductosUpdateAsync();
            var historicas = await _ventasHistoricasRepo.GetVentasHistoricasAsync();
            var csvOrders = await _ordersRepo.ReadFileAsync(_config["Csv:orders"]!);
            var csvVendedores = await _vendedorRepo.ReadFileAsync(_config["Csv:vendedores"]!);

            await CargarDimClienteAsync(csvCustomers, apiClientes, fuentes, ahora);
            await CargarDimProductoAsync(csvProducts, apiProductos, fuentes, ahora);
            await CargarDimTiempoAsync(historicas, csvOrders);
            await CargarDimVendedorAsync(csvVendedores, fuentes, ahora);

            _logger.LogInformation("Dimensiones cargadas exitosamente.");
            return Result.Ok("Dimensiones cargadas exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en LoadDimsDataAsync");
            return Result.Fail($"Error en LoadDimsDataAsync: {ex.Message}");
        }
    }

    private async Task CleanDimensionTablesAsync()
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        await conn.ExecuteAsync("DELETE FROM [Fact].[FactVentas]");
        await conn.ExecuteAsync("DELETE FROM [Dimension].[DimCliente]");
        await conn.ExecuteAsync("DELETE FROM [Dimension].[DimProducto]");
        await conn.ExecuteAsync("DELETE FROM [Dimension].[DimTiempo]");
        await conn.ExecuteAsync("DELETE FROM [Dimension].[DimVendedor]");
        await conn.ExecuteAsync("DELETE FROM [Dimension].[DimFuente]");
    }

    private async Task<Dictionary<int, DimFuente>> CrearFuentesAsync(DateTime ahora)
    {
        var fuentesData = new (int Id, string Nombre, string Tipo)[]
        {
            (1, "CSV Productos", "CSV"),
            (2, "CSV Clientes", "CSV"),
            (3, "CSV Ventas", "CSV"),
            (4, "API REST", "API_REST"),
            (5, "BD Historica", "BD_EXTERNA")
        };

        var result = new Dictionary<int, DimFuente>();

        foreach (var (id, nombre, tipo) in fuentesData)
        {
            var existente = await _context.DimFuentes.FirstOrDefaultAsync(f => f.IdFuente == id);
            if (existente == null)
            {
                existente = new DimFuente
                {
                    IdFuente = id,
                    NombreFuente = nombre,
                    Tipo = tipo,
                    FechaCarga = ahora
                };
                _context.DimFuentes.Add(existente);
            }
            result[id] = existente;
        }

        await _context.SaveChangesAsync();
        return result;
    }

    private async Task CargarDimClienteAsync(
        IEnumerable<CustomersCsv> csvCustomers,
        IEnumerable<ClientesUpdate> apiClientes,
        Dictionary<int, DimFuente> fuentes,
        DateTime ahora)
    {
        var clientesDict = new Dictionary<int, DimCliente>();

        foreach (var c in csvCustomers)
        {
                clientesDict[c.CustomerID] = new DimCliente
                {
                    ClienteID = c.CustomerID,
                    Nombre = $"{c.FirstName} {c.LastName}".Trim(),
                    Pais = c.Country,
                    Ciudad = c.City,
                    Region = c.Region,
                    Segmento = DeterminarSegmento(c.Country),
                    FechaCarga = ahora,
                    IdFuente = fuentes[2].FuenteKey
                };
            }

            foreach (var c in apiClientes)
            {
                if (clientesDict.TryGetValue(c.CustomerID, out var existente))
                {
                    existente.Nombre = $"{c.FirstName} {c.LastName}".Trim();
                    existente.Pais = c.Country;
                    existente.Ciudad = c.City;
                    existente.IdFuente = fuentes[4].FuenteKey;
                }
                else
                {
                    clientesDict[c.CustomerID] = new DimCliente
                    {
                        ClienteID = c.CustomerID,
                        Nombre = $"{c.FirstName} {c.LastName}".Trim(),
                        Pais = c.Country,
                        Ciudad = c.City,
                        Region = DeterminarRegion(c.Country),
                        Segmento = DeterminarSegmento(c.Country),
                        FechaCarga = ahora,
                        IdFuente = fuentes[4].FuenteKey
                    };
                }
            }

        _context.DimClientes.AddRange(clientesDict.Values);
        await _context.SaveChangesAsync();
    }

    private async Task CargarDimProductoAsync(
        IEnumerable<ProductsCsv> csvProducts,
        IEnumerable<ProductosUpdate> apiProductos,
        Dictionary<int, DimFuente> fuentes,
        DateTime ahora)
    {
        var productosDict = new Dictionary<int, DimProducto>();

        foreach (var p in csvProducts)
        {
            productosDict[p.ProductID] = new DimProducto
            {
                ProductoID = p.ProductID,
                Nombre = p.ProductName,
                Categoria = p.Category,
                PrecioActual = p.Price,
                Stock = p.Stock,
                Marca = DeterminarMarca(p.ProductName),
                FechaCarga = ahora,
                IdFuente = fuentes[1].FuenteKey
            };
        }

        foreach (var p in apiProductos)
        {
            if (productosDict.TryGetValue(p.ProductID, out var existente))
            {
                existente.Nombre = p.ProductName;
                existente.Categoria = p.Category;
                existente.PrecioActual = p.Price;
                existente.Stock = p.Stock;
                existente.IdFuente = fuentes[4].FuenteKey;
            }
            else
            {
                productosDict[p.ProductID] = new DimProducto
                {
                    ProductoID = p.ProductID,
                    Nombre = p.ProductName,
                    Categoria = p.Category,
                    PrecioActual = p.Price,
                    Stock = p.Stock,
                    Marca = DeterminarMarca(p.ProductName),
                    FechaCarga = ahora,
                    IdFuente = fuentes[4].FuenteKey
                };
            }
        }

        _context.DimProductos.AddRange(productosDict.Values);
        await _context.SaveChangesAsync();
    }

    private async Task CargarDimTiempoAsync(
        IEnumerable<VentasHistoricas> historicas,
        IEnumerable<OrdersCsv> ordenesCsv)
    {
        var fechasHistoricas = historicas.Select(h => h.OrderDate.Date);
        var fechasCsv = ordenesCsv.Select(o => o.OrderDate.Date);

        var fechas = fechasHistoricas
            .Concat(fechasCsv)
            .Distinct()
            .OrderBy(f => f)
            .ToList();

        var existentes = (await _context.DimTiempos.Select(t => t.Fecha).ToListAsync()).ToHashSet();

        var nuevos = fechas
            .Where(f => !existentes.Contains(f))
            .Select(f => new DimTiempo
            {
                Fecha = f,
                Anio = f.Year,
                Mes = f.Month,
                NombreMes = f.ToString("MMMM"),
                Trimestre = (f.Month - 1) / 3 + 1,
                Semestre = f.Month <= 6 ? 1 : 2,
                SemanaAnio = System.Globalization.CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(f, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday),
                DiaMes = f.Day,
                DiaSemana = f.ToString("dddd"),
                EsFinSemana = f.DayOfWeek == DayOfWeek.Saturday || f.DayOfWeek == DayOfWeek.Sunday,
                EsFeriado = EsFeriado(f)
            })
            .ToList();

        if (nuevos.Count > 0)
        {
            _context.DimTiempos.AddRange(nuevos);
            await _context.SaveChangesAsync();
        }
    }

    private async Task CargarDimVendedorAsync(IEnumerable<VendedorCsv> vendedores, Dictionary<int, DimFuente> fuentes, DateTime ahora)
    {
        foreach (var v in vendedores)
        {
            var existente = await _context.DimVendedores.FirstOrDefaultAsync(e => e.VendedorID == v.VendedorId);
            if (existente == null)
            {
                _context.DimVendedores.Add(new DimVendedor
                {
                    VendedorID = v.VendedorId,
                    Nombre = v.Nombre,
                    Region = v.Region,
                    FechaCarga = ahora,
                    IdFuente = fuentes[3].FuenteKey
                });
            }
            else
            {
                existente.Nombre = v.Nombre;
                existente.Region = v.Region;
                existente.FechaCarga = ahora;
            }
        }

        await _context.SaveChangesAsync();
    }

    private static string DeterminarRegion(string pais)
    {
        return pais?.ToLower() switch
        {
            "usa" or "estados unidos" or "united states" => "Norte",
            "republica dominicana" or "rd" => "Caribe",
            _ => "General"
        };
    }

    private static string DeterminarSegmento(string pais)
    {
        return pais?.ToLower() switch
        {
            "republica dominicana" or "rd" => "Local",
            "usa" or "estados unidos" or "united states" => "Internacional",
            _ => "General"
        };
    }

    private static string DeterminarMarca(string productName)
    {
        if (string.IsNullOrWhiteSpace(productName)) return "Generica";
        if (productName.Contains("Premium", StringComparison.OrdinalIgnoreCase)) return "Premium";
        if (productName.Contains("Organico", StringComparison.OrdinalIgnoreCase)) return "Organico";
        if (productName.Contains("Integral", StringComparison.OrdinalIgnoreCase)) return "Saludable";
        return "Generica";
    }

    private static bool EsFeriado(DateTime fecha)
    {
        var feriados = new HashSet<(int, int)>
        {
            (1, 1), (1, 21), (2, 27), (5, 1), (8, 16), (9, 24), (12, 25)
        };
        return feriados.Contains((fecha.Month, fecha.Day));
    }
}