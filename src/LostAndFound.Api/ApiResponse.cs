using System.Collections.Generic;

namespace LostAndFound.Api;

public class ApiResponse<T>
{
    public bool Succeeded { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> Success(T data, string? message = "Operation Completed Successfully .")
    {
        return new ApiResponse<T>
        {
            Succeeded = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> Failure(List<string> errors, string? message = "Operation Failed .")
    {
        return new ApiResponse<T>
        {
            Succeeded = false,
            Message = message,
            Errors = errors
        };
    }

    public static ApiResponse<T> Failure(string error, string? message = "Operation Failed .")
    {
        return new ApiResponse<T>
        {
            Succeeded = false,
            Message = message,
            Errors = new List<string> { error }
        };
    }
}