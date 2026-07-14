using SAV.application.Resultado;

namespace SAV.application.Interfaces;

public interface IDataWarehouseService
{
    Task<Result> ProcessDimensionsLoadAsync();
    Task<Result> ProcessFactsLoadAsync();
}
