namespace ShoppingCartExample.Polly;

[GenerateSerializer]
public class Result<T>
{
    [Id(0)]
    public bool IsSuccess { get; }

    [Id(1)]
    public T Data { get; }

    [Id(2)]
    public string Error { get; }

    protected Result(T data, bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
    }

    public static Result<T> Success(T data) => new Result<T>(data, true, string.Empty);
    public static Result<T> Failure(string error) => new Result<T>(default, false, error);
}