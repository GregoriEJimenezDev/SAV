using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SAV.application.Repository;
using SAV.application.Resultado;
using SAV.domain.Entities.Csv;
using SAV.domain.Entities.Data_Warehouse.Facts;

namespace SAV.persistencia.Repositorios.Data_Warehouse;

public class FactLoader : IFactLoader
{
    private readonly DwContext _context;
    private readonly IordersCsvRepo _ordersRepo;
    private readonly IorderDetailsCsvRepo _orderDetailsRepo;
    private readonly IVentasHistoricasDBRepo _ventasHistoricasRepo;
    private readonly IConfiguration _config;
    private readonly ILogger<FactLoader> _logger;

    public FactLoader(
        DwContext context,
        IConfiguration config,
        IordersCsvRepo ordersRepo,
        IorderDetailsCsvRepo orderDetailsRepo,
        IVentasHistoricasDBRepo ventasHistoricasRepo,
        ILogger<FactLoader> logger)
    {
        _context = context;
        _ordersRepo = ordersRepo;
        _orderDetailsRepo = orderDetailsRepo;
        _ventasHistoricasRepo = ventasHistoricasRepo;
        _config = config;
        _logger = logger;
    }

    public async Task<Result> LoadFactsDataAsync()
    {
        try
        {
            _logger.LogInformation("Cargando hechos (FactVentas)...");
            var ahora = DateTime.Now;
            var historicas = await _ventasHistoricasRepo.GetVentasHistoricasAsync();
            var csvOrders = await _ordersRepo.ReadFileAsync(_config["Csv:orders"]!);
            var csvDetails = await _orderDetailsRepo.ReadFileAsync(_config["Csv:order_details"]!);

            var fechaIds = historicas.Select(h => h.OrderDate.Date)
                .Concat(csvOrders.Select(o => o.OrderDate.Date))
                .Distinct()
                .ToHashSet();

            var tiempos = await _context.DimTiempos
                .Where(t => fechaIds.Contains(t.Fecha))
                .ToDictionaryAsync(t => t.Fecha, t => t.TiempoKey);

            var productos = await _context.DimProductos
                .ToDictionaryAsync(p => p.ProductoID, p => p.ProductoKey);

            var clientes = await _context.DimClientes
                .ToDictionaryAsync(c => c.ClienteID, c => c.ClienteKey);

            var vendedoresDict = await _context.DimVendedores
                .ToDictionaryAsync(v => v.VendedorID, v => v.VendedorKey);
            var vendedorDefaultKey = vendedoresDict.Values.FirstOrDefault();

            var fuenteKey = (await _context.DimFuentes
                .FirstOrDefaultAsync(f => f.IdFuente == 4))?.FuenteKey ?? 1;

            var facts = new List<FactVentas>();

            foreach (var h in historicas)
            {
                if (!tiempos.TryGetValue(h.OrderDate.Date, out var tKey))
                {
                    _logger.LogWarning("HistoricalSales OrderID {OrderID}: saltado, fecha {Date} sin DimTiempo", h.OrderID, h.OrderDate.Date);
                    continue;
                }
                if (!productos.TryGetValue(h.ProductID, out var pKey))
                {
                    _logger.LogWarning("HistoricalSales OrderID {OrderID}: saltado, ProductID {PID} sin DimProducto", h.OrderID, h.ProductID);
                    continue;
                }
                if (!clientes.TryGetValue(h.CustomerID, out var cKey))
                {
                    _logger.LogWarning("HistoricalSales OrderID {OrderID}: saltado, CustomerID {CID} sin DimCliente", h.OrderID, h.CustomerID);
                    continue;
                }

                facts.Add(new FactVentas
                {
                    TiempoKey = tKey,
                    ProductoKey = pKey,
                    ClienteKey = cKey,
                    VendedorKey = vendedorDefaultKey,
                    FuenteKey = fuenteKey,
                    FacturaId = $"HIST-{h.OrderID}",
                    Cantidad = h.Quantity,
                    PrecioUnitario = h.UnitPrice,
                    Total = h.TotalPrice,
                    Status = h.Status,
                    FechaCarga = ahora
                });
            }

            var ordenesMap = csvOrders.ToDictionary(o => o.OrderID);
            var csvFuenteKey = (await _context.DimFuentes
                .FirstOrDefaultAsync(f => f.IdFuente == 3))?.FuenteKey ?? 1;

            foreach (var d in csvDetails)
            {
                if (!ordenesMap.TryGetValue(d.OrderID, out var order))
                {
                    _logger.LogWarning("OrderDetail OrderID {OID}: saltado, orden no existe en CSV orders", d.OrderID);
                    continue;
                }
                if (!tiempos.TryGetValue(order.OrderDate.Date, out var tKey))
                {
                    _logger.LogWarning("OrderDetail OrderID {OID}: saltado, fecha {Date} sin DimTiempo", d.OrderID, order.OrderDate.Date);
                    continue;
                }
                if (!productos.TryGetValue(d.ProductID, out var pKey))
                {
                    _logger.LogWarning("OrderDetail OrderID {OID}: saltado, ProductID {PID} sin DimProducto", d.OrderID, d.ProductID);
                    continue;
                }
                if (!clientes.TryGetValue(order.CustomerID, out var cKey))
                {
                    _logger.LogWarning("OrderDetail OrderID {OID}: saltado, CustomerID {CID} sin DimCliente", d.OrderID, order.CustomerID);
                    continue;
                }

                if (!vendedoresDict.TryGetValue(order.VendedorId, out var venKey))
                {
                    _logger.LogWarning("OrderDetail OrderID {OID}: saltado, VendedorId {VID} sin DimVendedor", d.OrderID, order.VendedorId);
                    continue;
                }

                facts.Add(new FactVentas
                {
                    TiempoKey = tKey,
                    ProductoKey = pKey,
                    ClienteKey = cKey,
                    VendedorKey = venKey,
                    FuenteKey = csvFuenteKey,
                    FacturaId = $"CSV-{d.OrderID}",
                    Cantidad = d.Quantity,
                    PrecioUnitario = d.UnitPrice,
                    Total = d.Quantity * d.UnitPrice,
                    Status = order.Status,
                    FechaCarga = ahora
                });
            }

            await InsertFactVentasBatchAsync(facts);

            _logger.LogInformation("{Count} registros de FactVentas insertados.", facts.Count);
            return Result.Ok($"{facts.Count} registros de FactVentas insertados.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en LoadFactsDataAsync");
            return Result.Fail($"Error en LoadFactsDataAsync: {ex.Message}");
        }
    }

    private async Task InsertFactVentasBatchAsync(List<FactVentas> facts)
    {
        const int batchSize = 5000;

        for (int i = 0; i < facts.Count; i += batchSize)
        {
            var batch = facts.Skip(i).Take(batchSize).ToList();
            _context.FactVentas.AddRange(batch);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
        }
    }
}