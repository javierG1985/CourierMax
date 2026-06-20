using Ceiba.CourierMax.Application.Features;
using Ceiba.CourierMax.Application.Interfaces;
using Ceiba.CourierMax.Domain.Models;
using Ceiba.CourierMax.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ceiba.CourierMax.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class DriversController() : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ModelResponse), 200)]
    public async Task<IActionResult> GetAll(
        [FromServices] IDriverRepository driverRepository,
        CancellationToken ct)
    {
        var drivers = await driverRepository.GetAllAsync(ct);
        var result = drivers.Select(d => new
        {
            d.Id,
            d.Name,
            d.IsActive,
            d.VehicleId,
            Vehicle = d.Vehicle == null ? null : new
            {
                d.Vehicle.Id,
                d.Vehicle.Plate,
                d.Vehicle.MaxWeightKg,
                d.Vehicle.MaxVolumeM3
            }
        });
        return StatusCode(StatusCodes.Status200OK, ReponseApiService.Response(
            StatusCodes.Status200OK, "Conductores obtenidos exitosamente.", result));
    }

    [HttpGet("{id:guid}/metrics")]
    [ProducesResponseType(typeof(ModelResponse), 200)]
    [ProducesResponseType(typeof(ModelResponse), 404)]
    public async Task<IActionResult> GetMetrics(
        Guid id,
        [FromServices] IGetDriverMetricsUseCase getDriverMetrics,
        CancellationToken ct)
    {
        var result = await getDriverMetrics.ExecuteForDriverAsync(id, ct);
        return StatusCode(StatusCodes.Status200OK, ReponseApiService.Response(
            StatusCodes.Status200OK, "Métricas del conductor obtenidas.", result));
    }

    [HttpGet("metrics")]
    [ProducesResponseType(typeof(ModelResponse), 200)]
    public async Task<IActionResult> GetAllMetrics(
        [FromServices] IGetDriverMetricsUseCase getDriverMetrics,
        CancellationToken ct)
    {
        var result = await getDriverMetrics.ExecuteAsync(ct);
        return StatusCode(StatusCodes.Status200OK, ReponseApiService.Response(
            StatusCodes.Status200OK, "Métricas de todos los conductores obtenidas.", result));
    }
}
