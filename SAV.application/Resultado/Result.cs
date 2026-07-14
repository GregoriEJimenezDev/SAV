namespace SAV.application.Resultado;

public class Result
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;

    public static Result Ok() => new() { IsSuccess = true };
    public static Result Ok(string message) => new() { IsSuccess = true, Message = message };
    public static Result Fail(string message) => new() { IsSuccess = false, Message = message };
}
