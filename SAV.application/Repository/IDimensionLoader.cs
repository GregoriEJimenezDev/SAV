using SAV.application.Resultado;

namespace SAV.application.Repository;

public interface IDimensionLoader
{
    Task<Result> LoadDimsDataAsync();
}