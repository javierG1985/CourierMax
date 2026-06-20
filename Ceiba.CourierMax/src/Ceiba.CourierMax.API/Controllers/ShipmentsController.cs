using Ceiba.CourierMax.Application.DTOs.Requests;
using Ceiba.CourierMax.Application.Features;
using Ceiba.CourierMax.Application.Interfaces;
using Ceiba.CourierMax.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ceiba.CourierMax.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class ShipmentsController() : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ModelResponse), 201)]
    public async Task<IActionResult> Create(
        [FromBody] CreateShipmentRequest request,
        [FromServices] ICreateShipmentUseCase createShipment,
        CancellationToken ct)
    {
        var result = await createShipment.ExecuteAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, ReponseApiService.Response(
            StatusCodes.Status201Created, "Envío creado exitosamente.", result));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ModelResponse), 200)]
    public async Task<IActionResult> GetAll(
        [FromServices] IGetShipmentUseCase getShipment,
        CancellationToken ct)
    {
        var result = await getShipment.GetAllAsync(ct);
        return StatusCode(StatusCodes.Status200OK, ReponseApiService.Response(
            StatusCodes.Status200OK, "Envíos obtenidos exitosamente.", result));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ModelResponse), 200)]
    [ProducesResponseType(typeof(ModelResponse), 404)]
    public async Task<IActionResult> GetById(
        Guid id,
        [FromServices] IGetShipmentUseCase getShipment,
        CancellationToken ct)
    {
        var result = await getShipment.GetByIdAsync(id, ct);
        return StatusCode(StatusCodes.Status200OK, ReponseApiService.Response(
            StatusCodes.Status200OK, "Envío obtenido exitosamente.", result));
    }

    [HttpGet("tracking/{code}")]
    [ProducesResponseType(typeof(ModelResponse), 200)]
    [ProducesResponseType(typeof(ModelResponse), 404)]
    public async Task<IActionResult> GetByTrackingCode(
        string code,
        [FromServices] IGetShipmentUseCase getShipment,
        CancellationToken ct)
    {
        var result = await getShipment.GetByTrackingCodeAsync(code, ct);
        return StatusCode(StatusCodes.Status200OK, ReponseApiService.Response(
            StatusCodes.Status200OK, "Envío encontrado.", result));
    }

    [HttpPost("{id:guid}/assign")]
    [ProducesResponseType(typeof(ModelResponse), 200)]
    [ProducesResponseType(typeof(ModelResponse), 422)]
    public async Task<IActionResult> Assign(
        Guid id,
        [FromBody] AssignShipmentRequest request,
        [FromServices] IAssignShipmentUseCase assignShipment,
        CancellationToken ct)
    {
        var resolvedRequest = request with { ShipmentId = id };
        var result = await assignShipment.ExecuteAsync(resolvedRequest, ct);
        return StatusCode(StatusCodes.Status200OK, ReponseApiService.Response(
            StatusCodes.Status200OK, "Envío asignado al conductor.", result));
    }

    [HttpPut("{id:guid}/transit")]
    [ProducesResponseType(typeof(ModelResponse), 200)]
    [ProducesResponseType(typeof(ModelResponse), 422)]
    public async Task<IActionResult> StartTransit(
        Guid id,
        [FromBody] ChangeStatusRequest request,
        [FromServices] IChangeShipmentStatusUseCase changeStatus,
        CancellationToken ct)
    {
        var resolvedRequest = request with { ShipmentId = id };
        var result = await changeStatus.StartTransitAsync(resolvedRequest, ct);
        return StatusCode(StatusCodes.Status200OK, ReponseApiService.Response(
            StatusCodes.Status200OK, "Envío marcado en tránsito.", result));
    }

    [HttpPut("{id:guid}/deliver")]
    [ProducesResponseType(typeof(ModelResponse), 200)]
    [ProducesResponseType(typeof(ModelResponse), 422)]
    public async Task<IActionResult> Deliver(
        Guid id,
        [FromBody] ChangeStatusRequest request,
        [FromServices] IChangeShipmentStatusUseCase changeStatus,
        CancellationToken ct)
    {
        var resolvedRequest = request with { ShipmentId = id };
        var result = await changeStatus.DeliverAsync(resolvedRequest, ct);
        return StatusCode(StatusCodes.Status200OK, ReponseApiService.Response(
            StatusCodes.Status200OK, "Envío marcado como entregado.", result));
    }

    [HttpPut("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ModelResponse), 200)]
    [ProducesResponseType(typeof(ModelResponse), 422)]
    public async Task<IActionResult> Cancel(
        Guid id,
        [FromBody] CancelShipmentRequest request,
        [FromServices] ICancelShipmentUseCase cancelShipment,
        CancellationToken ct)
    {
        var resolvedRequest = request with { ShipmentId = id };
        var result = await cancelShipment.ExecuteAsync(resolvedRequest, ct);
        return StatusCode(StatusCodes.Status200OK, ReponseApiService.Response(
            StatusCodes.Status200OK, "Envío cancelado.", result));
    }
}
