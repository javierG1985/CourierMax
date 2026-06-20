using Ceiba.CourierMax.Application.Interfaces;
using Ceiba.CourierMax.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ceiba.CourierMax.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class ReportsController() : ControllerBase
{
    [HttpGet("delayed")]
    [ProducesResponseType(typeof(ModelResponse), 200)]
    [ProducesResponseType(typeof(ModelResponse), 400)]
    public async Task<IActionResult> GetDelayed(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromServices] IGetDelayedShipmentsUseCase getDelayedShipments,
        CancellationToken ct)
    {
        if (from > to)
            return StatusCode(StatusCodes.Status400BadRequest, ResponseApiService.Response(
                StatusCodes.Status400BadRequest, "La fecha 'Inicial' debe ser menor a 'Final'."));

        var result = await getDelayedShipments.ExecuteAsync(from, to, ct);
        return StatusCode(StatusCodes.Status200OK, ResponseApiService.Response(
            StatusCodes.Status200OK, "Envíos atrasados obtenidos.", result));
    }
}
