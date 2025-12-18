using tennis_wave_api.Models;

namespace tennis_wave_api.Helpers;

public static class ApiResponseHelper
{
    public static ApiResponse<T> Success<T>(T data, string message = "Success")
    {
        return new ApiResponse<T>
        {
            Code = 0,
            Message = message,
            Data = data,
        };
    }

    public static ApiResponse<T> Fail<T>(string message, int code = 1, T? data = default)
    {
        return new ApiResponse<T>
        {
            Code = code,
            Message = message,
            Data = data,
        };
    }
    
    public static ApiResponse<T> Error<T>(string message, int code = 1, T? data = default)
    {
        return Fail<T>(message, code, data);
    }

    public static ApiResponse<object> Error(string message, int code = 1)
    {
        return Error<object>(message, code, null);
    }
}