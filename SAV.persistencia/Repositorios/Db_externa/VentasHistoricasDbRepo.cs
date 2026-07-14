using Microsoft.EntityFrameworkCore;
using SAV.application.Repository;
using SAV.domain.Entities.DB_Externa;

namespace SAV.persistencia.Repositorios.Db_externa;

public class VentasHistoricasDbRepo : IVentasHistoricasDBRepo
{
    private readonly VentasHistoricasContext _context;

    public VentasHistoricasDbRepo(VentasHistoricasContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<VentasHistoricas>> GetVentasHistoricasAsync()
    {
        return await _context.HistoricalSales.ToListAsync();
    }
}
