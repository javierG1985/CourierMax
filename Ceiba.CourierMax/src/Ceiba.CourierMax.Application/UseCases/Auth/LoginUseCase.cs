using Ceiba.CourierMax.Application.DTOs.Requests;
using Ceiba.CourierMax.Application.DTOs.Responses;
using Ceiba.CourierMax.Application.Interfaces;
using Ceiba.CourierMax.Domain.Exceptions;

namespace Ceiba.CourierMax.Application.UseCases.Auth;

public sealed class LoginUseCase(IUserService userService) : ILoginUseCase
{
    public async Task<LoginResponse> ExecuteAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await userService.FindByUsernameAsync(request.Username, ct)
            ?? throw new DomainException("Credenciales inválidas.");

        if (!userService.VerifyPassword(request.Password, user.PasswordHash))
            throw new DomainException("Credenciales inválidas.");

        return new LoginResponse(user.Username, user.Role);
    }
}
