using Microsoft.EntityFrameworkCore;
using SAV.domain.Entities.DB_Externa;

namespace SAV.persistencia.Repositorios.Db_externa;

public class VentasHistoricasContext : DbContext
{
    public VentasHistoricasContext(DbContextOptions<VentasHistoricasContext> options) : base(options) { }

    public DbSet<VentasHistoricas> HistoricalSales => Set<VentasHistoricas>();
}
