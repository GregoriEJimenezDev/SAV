using Microsoft.EntityFrameworkCore;
using SAV.application.Interfaces;

namespace SAV.workerLoad;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker ETL iniciado.");

        var runAtStartup = _configuration.GetValue<bool>("WorkerSettings:RunAtStartup");
        var intervalMinutes = _configuration.GetValue<int>("WorkerSettings:IntervalMinutes");

        if (intervalMinutes <= 0) intervalMinutes = 60;

        if (!runAtStartup)
        {
            _logger.LogInformation("Esperando {delay}s antes de primera ejecucion...", 10);
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await EjecutarEtlAsync(stoppingToken);
            _logger.LogInformation("Proximo ETL en {min} minutos.", intervalMinutes);
            await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
        }
    }

    private async Task EjecutarEtlAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("===== INICIO ETL =====");

            using var scope = _serviceProvider.CreateScope();
            var dwService = scope.ServiceProvider.GetRequiredService<IDataWarehouseService>();

            var dimResult = await dwService.ProcessDimensionsLoadAsync();
            _logger.LogInformation("Dimensiones: {message}", dimResult.Message);

            if (dimResult.IsSuccess)
            {
                var factResult = await dwService.ProcessFactsLoadAsync();
                _logger.LogInformation("Hechos: {message}", factResult.Message);
            }

            await VerificarDatosAsync(scope.ServiceProvider);

            _logger.LogInformation("===== ETL COMPLETADO =====");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante ejecucion ETL");
        }
    }

    private async Task VerificarDatosAsync(IServiceProvider serviceProvider)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<SAV.persistencia.Repositorios.Data_Warehouse.DwContext>();

            _logger.LogInformation("--- Verificacion de Datos ---");
            _logger.LogInformation("DimCliente:   {count}", await ctx.DimClientes.CountAsync());
            _logger.LogInformation("DimProducto:  {count}", await ctx.DimProductos.CountAsync());
            _logger.LogInformation("DimTiempo:    {count}", await ctx.DimTiempos.CountAsync());
            _logger.LogInformation("DimVendedor:  {count}", await ctx.DimVendedores.CountAsync());
            _logger.LogInformation("DimFuente:    {count}", await ctx.DimFuentes.CountAsync());
            _logger.LogInformation("FactVentas:   {count}", await ctx.FactVentas.CountAsync());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al verificar datos");
        }
    }
}
