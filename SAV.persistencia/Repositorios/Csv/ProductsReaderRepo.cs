using System.Globalization;
using CsvHelper;
using Microsoft.Extensions.Configuration;
using SAV.application.Repository;
using SAV.domain.Entities.Csv;

namespace SAV.persistencia.Repositorios.Csv;

public sealed class ProductsReaderRepo : IproductsCsvRepo
{
    private readonly string _archivo;

    public ProductsReaderRepo(IConfiguration config)
    {
        _archivo = config["Csv:products"] ?? throw new InvalidOperationException("Csv:products no configurado.");
    }

    public async Task<IEnumerable<ProductsCsv>> ReadFileAsync(string archivo)
    {
        var resultado = new List<ProductsCsv>();
        using var reader = new StreamReader(_archivo);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        await foreach (var record in csv.GetRecordsAsync<ProductsCsv>())
            resultado.Add(record);
        return resultado;
    }
}
