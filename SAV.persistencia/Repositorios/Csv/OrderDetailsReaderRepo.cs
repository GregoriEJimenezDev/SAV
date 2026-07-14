using System.Globalization;
using CsvHelper;
using Microsoft.Extensions.Configuration;
using SAV.application.Repository;
using SAV.domain.Entities.Csv;

namespace SAV.persistencia.Repositorios.Csv;

public sealed class OrderDetailsReaderRepo : IorderDetailsCsvRepo
{
    private readonly string _archivo;

    public OrderDetailsReaderRepo(IConfiguration config)
    {
        _archivo = config["Csv:order_details"] ?? throw new InvalidOperationException("Csv:order_details no configurado.");
    }

    public async Task<IEnumerable<OrderDetailsCsv>> ReadFileAsync(string archivo)
    {
        var resultado = new List<OrderDetailsCsv>();
        using var reader = new StreamReader(_archivo);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        await foreach (var record in csv.GetRecordsAsync<OrderDetailsCsv>())
            resultado.Add(record);
        return resultado;
    }
}
