using SAV.application.Resultado;

namespace SAV.application.Repository;

public interface IFactLoader
{
    Task<Result> LoadFactsDataAsync();
}