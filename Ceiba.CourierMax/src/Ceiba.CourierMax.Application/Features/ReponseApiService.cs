using Ceiba.CourierMax.Domain.Models;

namespace Ceiba.CourierMax.Application.Features
{
    public static class ReponseApiService
    {
       public static ModelResponse Response(int statusCode, string? message = null, dynamic? data = null)
        {
            bool success = false;

            if(statusCode >= 200 && statusCode < 300)
            {
                success = true;
            }

            return new ModelResponse
            {
                StatusCode = statusCode,
                Success = success,
                Message = message,
                Data = data
            };
        }
    }
}
