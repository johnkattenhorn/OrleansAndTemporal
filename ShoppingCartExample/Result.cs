namespace ShoppingCartExample;

[GenerateSerializer]
public class Result<T>
{

    public Result() { }

    [Id(0)]
    public bool IsSuccess { get; set; }

    [Id(1)] 
    public T Data { get; set; }

    [Id(2)] 
    public string Error { get; set; }

    protected Result(T data, bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
    }

    public static Result<T> Success(T data) => new Result<T>(data, true, string.Empty);
    public static Result<T> Failure(string error) => new Result<T>(default, false, error);
}