using Ceiba.CourierMax.Application.Models;

namespace Ceiba.CourierMax.API;

public static class ResponseApiService
{
    public static ModelResponse Response(int statusCode, string? message = null, dynamic? data = null)
    {
        bool success = statusCode >= 200 && statusCode < 300;

        return new ModelResponse
        {
            StatusCode = statusCode,
            Success = success,
            Message = message,
            Data = data
        };
    }
}
