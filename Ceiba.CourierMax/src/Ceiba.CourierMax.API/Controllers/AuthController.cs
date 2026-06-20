using Ceiba.CourierMax.API.Services.Interfaces;
using Ceiba.CourierMax.Application.DTOs.Requests;
using Ceiba.CourierMax.Application.Interfaces;
using Ceiba.CourierMax.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ceiba.CourierMax.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController() : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(ModelResponse), 200)]
    [ProducesResponseType(typeof(ModelResponse), 422)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        [FromServices] ILoginUseCase loginUseCase,
        [FromServices] IJwtTokenService jwtTokenService,
        CancellationToken ct)
    {
        var result = await loginUseCase.ExecuteAsync(request, ct);
        if(result == null)
        {
            return StatusCode(StatusCodes.Status422UnprocessableEntity, ResponseApiService.Response(
                StatusCodes.Status422UnprocessableEntity, "Credenciales inválidas"));
        }

        jwtTokenService.IssueToken(result.Username, result.Role);

        return StatusCode(StatusCodes.Status200OK, ResponseApiService.Response(
                StatusCodes.Status200OK, "Login exitoso", result));
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ModelResponse), 200)]
    public IActionResult Logout(
        [FromServices] IJwtTokenService jwtTokenService)
    {
        jwtTokenService.RevokeToken();

        return StatusCode(StatusCodes.Status200OK, ResponseApiService.Response(StatusCodes.Status200OK,"Sesión cerrada."));
    }
}
