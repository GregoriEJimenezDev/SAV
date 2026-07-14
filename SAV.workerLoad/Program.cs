using Microsoft.EntityFrameworkCore;
using SAV.application.Interfaces;
using SAV.application.Repository;
using SAV.application.Services;
using SAV.persistencia.Repositorios.Api;
using SAV.persistencia.Repositorios.Csv;
using SAV.persistencia.Repositorios.Data_Warehouse;
using SAV.persistencia.Repositorios.Db_externa;
using SAV.workerLoad;

var builder = Host.CreateApplicationBuilder(args);

var dwConn = builder.Configuration.GetConnectionString("DtwarehouseConnString")
    ?? throw new InvalidOperationException("DtwarehouseConnString no configurado.");
var extConn = builder.Configuration.GetConnectionString("DbExterna")
    ?? throw new InvalidOperationException("DbExterna no configurado.");

builder.Services.AddDbContext<DwContext>(options => options.UseSqlServer(dwConn));
builder.Services.AddDbContext<VentasHistoricasContext>(options => options.UseSqlServer(extConn));

builder.Services.AddHttpClient<IClientesUpdateApiRepo, ClientesUpdateApiRepo>();
builder.Services.AddHttpClient<IProductosUpdateApiRepo, ProductosUpdateApiRepo>();

builder.Services.AddScoped<IproductsCsvRepo, ProductsReaderRepo>();
builder.Services.AddScoped<IcustomersCsvRepo, CustomersReaderRepo>();
builder.Services.AddScoped<IordersCsvRepo, OrdersReaderRepo>();
builder.Services.AddScoped<IorderDetailsCsvRepo, OrderDetailsReaderRepo>();
builder.Services.AddScoped<IVendedorCsvRepo, VendedorReaderRepo>();
builder.Services.AddScoped<IVentasHistoricasDBRepo, VentasHistoricasDbRepo>();
builder.Services.AddScoped<IDimensionLoader, DimensionLoader>();
builder.Services.AddScoped<IFactLoader, FactLoader>();
builder.Services.AddScoped<IDataWarehouseService, DataWarehouseService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<DwContext>();
    await ctx.Database.EnsureCreatedAsync();

    var extCtx = scope.ServiceProvider.GetRequiredService<VentasHistoricasContext>();
    await extCtx.Database.EnsureCreatedAsync();
}

await host.RunAsync();