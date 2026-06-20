using Ceiba.CourierMax.Domain.Exceptions;
using System.Text.Json;

namespace Ceiba.CourierMax.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DomainException ex)
        {
            logger.LogWarning("Regla de negocio violada: {Message}", ex.Message);
            await WriteResponseAsync(context, StatusCodes.Status422UnprocessableEntity, ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning("Recurso no encontrado: {Message}", ex.Message);
            await WriteResponseAsync(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error inesperado");
            await WriteResponseAsync(context, StatusCodes.Status500InternalServerError,
                "Ocurrió un error inesperado. Intente más tarde.");
        }
    }

    private static async Task WriteResponseAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = ResponseApiService.Response(statusCode, message);
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
