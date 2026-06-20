using Ceiba.CourierMax.Persistence.Context;
using Ceiba.CourierMax.Persistence.Seed;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net.Http.Json;

namespace Ceiba.CourierMax.Tests.Integration;

public class CourierMaxWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    // Cliente pre-autenticado compartido por todos los tests del fixture
    public HttpClient AuthenticatedClient { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CourierMaxDbContext>();
        await db.Database.EnsureCreatedAsync();
        await DataSeeder.SeedAsync(db);

        // Crear el cliente con manejo de cookies y hacer login una sola vez
        AuthenticatedClient = CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });

        await AuthenticatedClient.PostAsJsonAsync("/api/auth/login",
            new { username = "admin", password = "Admin123!" });
    }

    public new async Task DisposeAsync()
    {
        AuthenticatedClient.Dispose();
        await _connection.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<CourierMaxDbContext>>();
            services.RemoveAll(typeof(IDbContextOptionsConfiguration<CourierMaxDbContext>));

            services.AddDbContext<CourierMaxDbContext>(options =>
                options.UseSqlite(_connection));
        });
    }
}
