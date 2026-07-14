using SAV.application.Interfaces;
using SAV.application.Repository;
using SAV.application.Resultado;

namespace SAV.application.Services;

public class DataWarehouseService : IDataWarehouseService
{
    private readonly IDimensionLoader _dimensionLoader;
    private readonly IFactLoader _factLoader;

    public DataWarehouseService(IDimensionLoader dimensionLoader, IFactLoader factLoader)
    {
        _dimensionLoader = dimensionLoader;
        _factLoader = factLoader;
    }

    public async Task<Result> ProcessDimensionsLoadAsync()
    {
        return await _dimensionLoader.LoadDimsDataAsync();
    }

    public async Task<Result> ProcessFactsLoadAsync()
    {
        return await _factLoader.LoadFactsDataAsync();
    }
}