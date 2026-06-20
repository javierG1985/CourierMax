using Ceiba.CourierMax.Application.DTOs.Requests;
using Ceiba.CourierMax.Application.Interfaces;
using Ceiba.CourierMax.Application.UseCases.Auth;
using Ceiba.CourierMax.Domain.Entities;
using Ceiba.CourierMax.Domain.Exceptions;

namespace Ceiba.CourierMax.Tests.Unit.Application;

public class LoginUseCaseTests
{
    private static LoginUseCase BuildUseCase(AppUser? user) =>
        new(new FakeUserService(user));

    private static AppUser BuildAdmin(string username, string plainPassword) =>
        AppUser.Create(Guid.NewGuid(), username, BCrypt.Net.BCrypt.HashPassword(plainPassword), "Admin");

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsUsernameAndRole()
    {
        var useCase = BuildUseCase(BuildAdmin("admin", "Admin123!"));

        var result = await useCase.ExecuteAsync(new LoginRequest("admin", "Admin123!"));

        Assert.Equal("admin", result.Username);
        Assert.Equal("Admin", result.Role);
    }

    [Fact]
    public async Task Login_WithUnknownUsername_ThrowsDomainException()
    {
        var useCase = BuildUseCase(null);

        await Assert.ThrowsAsync<DomainException>(
            () => useCase.ExecuteAsync(new LoginRequest("noexiste", "Admin123!")));
    }

    [Fact]
    public async Task Login_WithWrongPassword_ThrowsDomainException()
    {
        var useCase = BuildUseCase(BuildAdmin("admin", "Admin123!"));

        await Assert.ThrowsAsync<DomainException>(
            () => useCase.ExecuteAsync(new LoginRequest("admin", "ClaveIncorrecta")));
    }

    private sealed class FakeUserService(AppUser? user) : IUserService
    {
        public Task<AppUser?> FindByUsernameAsync(string username, CancellationToken ct = default)
            => Task.FromResult(user);

        public bool VerifyPassword(string plainPassword, string passwordHash)
            => BCrypt.Net.BCrypt.Verify(plainPassword, passwordHash);
    }
}
