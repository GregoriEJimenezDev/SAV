using Microsoft.EntityFrameworkCore;
using SAV.domain.Entities.Api;

namespace SAV.API.Datos.Context;

public class ApiContext : DbContext
{
    public ApiContext(DbContextOptions<ApiContext> options) : base(options) { }

    public DbSet<ClientesUpdate> Clientes => Set<ClientesUpdate>();
    public DbSet<ProductosUpdate> Productos => Set<ProductosUpdate>();
}
