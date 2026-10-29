namespace MarketplaceApi.Services;

public class ServiceResult<T>
{
    public bool Success { get; init; }
    public int Status { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public Dictionary<string, string[]>? Errors { get; init; }

    public static ServiceResult<T> Ok(T data, int status = 200, string message = "OK") =>
        new() { Success = true, Status = status, Message = message, Data = data };

    public static ServiceResult<T> Fail(int status, string message, Dictionary<string, string[]>? errors = null) =>
        new() { Success = false, Status = status, Message = message, Errors = errors };
}
