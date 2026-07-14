using System.Globalization;
using CsvHelper;
using Microsoft.Extensions.Configuration;
using SAV.application.Repository;
using SAV.domain.Entities.Csv;

namespace SAV.persistencia.Repositorios.Csv;

public sealed class OrdersReaderRepo : IordersCsvRepo
{
    private readonly string _archivo;

    public OrdersReaderRepo(IConfiguration config)
    {
        _archivo = config["Csv:orders"] ?? throw new InvalidOperationException("Csv:orders no configurado.");
    }

    public async Task<IEnumerable<OrdersCsv>> ReadFileAsync(string archivo)
    {
        var resultado = new List<OrdersCsv>();
        using var reader = new StreamReader(_archivo);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        await foreach (var record in csv.GetRecordsAsync<OrdersCsv>())
            resultado.Add(record);
        return resultado;
    }
}
