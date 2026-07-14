using System.Globalization;
using CsvHelper;
using Microsoft.Extensions.Configuration;
using SAV.application.Repository;
using SAV.domain.Entities.Csv;

namespace SAV.persistencia.Repositorios.Csv;

public sealed class VendedorReaderRepo : IVendedorCsvRepo
{
    private readonly string _archivo;

    public VendedorReaderRepo(IConfiguration config)
    {
        _archivo = config["Csv:vendedores"] ?? throw new InvalidOperationException("Csv:vendedores no configurado.");
    }

    public async Task<IEnumerable<VendedorCsv>> ReadFileAsync(string archivo)
    {
        var resultado = new List<VendedorCsv>();
        using var reader = new StreamReader(_archivo);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        await foreach (var record in csv.GetRecordsAsync<VendedorCsv>())
            resultado.Add(record);
        return resultado;
    }
}