using System.Collections.Generic;

namespace ResumeAnalyzer.DTOs;

public class ServiceResult<T>
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> Errors { get; set; } = new();
    public T? Data { get; set; }

    public static ServiceResult<T> Success(T data) => new() { IsSuccess = true, Data = data };
    public static ServiceResult<T> Failure(string errorMessage, List<string>? errors = null) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage,
        Errors = errors ?? new List<string>()
    };
}
