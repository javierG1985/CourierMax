using Ceiba.CourierMax.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
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
            await WriteProblemAsync(context, StatusCodes.Status422UnprocessableEntity,
                "Regla de negocio", ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning("Recurso no encontrado: {Message}", ex.Message);
            await WriteProblemAsync(context, StatusCodes.Status404NotFound,
                "No encontrado", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error inesperado");
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError,
                "Error interno", "Ocurrió un error inesperado. Intente más tarde.");
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, int statusCode, string title, string detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
    }
}
