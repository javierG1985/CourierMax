using Ceiba.CourierMax.Application.DTOs.Requests;
using Ceiba.CourierMax.Application.DTOs.Responses;

namespace Ceiba.CourierMax.Application.Interfaces;

public interface ILoginUseCase
{
    Task<LoginResponse> ExecuteAsync(LoginRequest request, CancellationToken ct = default);
}
