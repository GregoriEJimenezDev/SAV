using System.Globalization;
using CsvHelper;
using Microsoft.Extensions.Configuration;
using SAV.application.Repository;
using SAV.domain.Entities.Csv;

namespace SAV.persistencia.Repositorios.Csv;

public sealed class CustomersReaderRepo : IcustomersCsvRepo
{
    private readonly string _archivo;

    public CustomersReaderRepo(IConfiguration config)
    {
        _archivo = config["Csv:customers"] ?? throw new InvalidOperationException("Csv:customers no configurado.");
    }

    public async Task<IEnumerable<CustomersCsv>> ReadFileAsync(string archivo)
    {
        var resultado = new List<CustomersCsv>();
        using var reader = new StreamReader(_archivo);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        await foreach (var record in csv.GetRecordsAsync<CustomersCsv>())
            resultado.Add(record);
        return resultado;
    }
}
