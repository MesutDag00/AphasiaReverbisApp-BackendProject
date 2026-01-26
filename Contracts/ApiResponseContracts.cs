namespace AphaisaReverbes.Contracts;

public sealed record ApiError(string Message);

public sealed record ApiResponse<T>(bool Success, T? Data, ApiError? Error)
{
    public static ApiResponse<T> Ok(T data) => new(true, data, null);
    public static ApiResponse<T> Fail(string message) => new(false, default, new ApiError(message));
}

