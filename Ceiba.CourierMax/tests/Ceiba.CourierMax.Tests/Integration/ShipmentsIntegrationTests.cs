using Ceiba.CourierMax.Persistence.Seed;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Ceiba.CourierMax.Tests.Integration;

public class ShipmentsIntegrationTests(CourierMaxWebFactory factory)
    : IClassFixture<CourierMaxWebFactory>
{
    private readonly HttpClient _client = factory.AuthenticatedClient;

    private static object BuildCreateRequest(
        string serviceType = "Express",
        string packageType = "Paquete",
        decimal weight = 5,
        string origin = "Bogota",
        string destination = "Medellin") => new
        {
            SenderName = "Remitente Test",
            SenderPhone = "3001234567",
            SenderAddress = "Calle 1 #10-20",
            RecipientName = "Destinatario Test",
            RecipientPhone = "3109876543",
            RecipientAddress = "Carrera 5 #20-30",
            WeightKg = weight,
            LengthCm = 30,
            WidthCm = 20,
            HeightCm = 15,
            PackageType = packageType,
            ServiceType = serviceType,
            OriginCity = origin,
            DestinationCity = destination
        };

    // Helpers para navegar la envoltura ModelResponse
    private static JsonElement Data(JsonElement body) => body.GetProperty("data");
    private static string DataString(JsonElement body, string prop) => Data(body).GetProperty(prop).GetString()!;

    [Fact]
    public async Task CreateShipment_ShouldReturn201_WithTrackingCode()
    {
        var response = await _client.PostAsJsonAsync("/api/shipments", BuildCreateRequest());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var trackingCode = DataString(body, "trackingCode");

        Assert.Matches(@"^CM\d{8}$", trackingCode);
    }

    [Fact]
    public async Task CreateShipment_Fragil5kg_Express_BogotaMedellin_ShouldCalculateFareCorrectly()
    {
        var response = await _client.PostAsJsonAsync("/api/shipments",
            BuildCreateRequest(serviceType: "Express", packageType: "Fragil", weight: 5));

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var fare = Data(body).GetProperty("fare").GetDecimal();

        Assert.Equal(40_950m, fare);
    }

    [Fact]
    public async Task GetShipmentByTrackingCode_ShouldReturnShipment()
    {
        var created = await _client.PostAsJsonAsync("/api/shipments", BuildCreateRequest());
        var body = await created.Content.ReadFromJsonAsync<JsonElement>();
        var code = DataString(body, "trackingCode");

        var response = await _client.GetAsync($"/api/shipments/tracking/{code}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AssignShipment_ShouldTransitionToAsignado()
    {
        var created = await _client.PostAsJsonAsync("/api/shipments", BuildCreateRequest());
        var body = await created.Content.ReadFromJsonAsync<JsonElement>();
        var shipmentId = DataString(body, "id");

        var assignResponse = await _client.PostAsJsonAsync(
            $"/api/shipments/{shipmentId}/assign",
            new { DriverId = DataSeeder.DriverJuanId, AssignedBy = "operador1" });

        Assert.Equal(HttpStatusCode.OK, assignResponse.StatusCode);

        var assignBody = await assignResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("ASIGNADO", DataString(assignBody, "status"));
    }

    [Fact]
    public async Task FullShipmentLifecycle_ShouldTransitionAllStates()
    {
        var created = await _client.PostAsJsonAsync("/api/shipments", BuildCreateRequest());
        var body = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = DataString(body, "id");

        await _client.PostAsJsonAsync($"/api/shipments/{id}/assign",
            new { DriverId = DataSeeder.DriverCarlosId, AssignedBy = "op1" });

        await _client.PutAsJsonAsync($"/api/shipments/{id}/transit",
            new { ChangedBy = "op1" });

        var delivered = await _client.PutAsJsonAsync($"/api/shipments/{id}/deliver",
            new { ChangedBy = "op1" });

        var deliveredBody = await delivered.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("ENTREGADO", DataString(deliveredBody, "status"));
        Assert.Equal(4, Data(deliveredBody).GetProperty("statusChanges").GetArrayLength());
    }

    [Fact]
    public async Task CancelShipment_WhenEntregado_ShouldReturn422()
    {
        var created = await _client.PostAsJsonAsync("/api/shipments", BuildCreateRequest());
        var body = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = DataString(body, "id");

        await _client.PostAsJsonAsync($"/api/shipments/{id}/assign",
            new { DriverId = DataSeeder.DriverMariaId, AssignedBy = "op1" });
        await _client.PutAsJsonAsync($"/api/shipments/{id}/transit", new { ChangedBy = "op1" });
        await _client.PutAsJsonAsync($"/api/shipments/{id}/deliver", new { ChangedBy = "op1" });

        var cancelResponse = await _client.PutAsJsonAsync($"/api/shipments/{id}/cancel",
            new { Reason = "Intento de cancelación tras entrega", CancelledBy = "op1" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, cancelResponse.StatusCode);
    }

    [Fact]
    public async Task AssignShipment_WhenExceedsVehicleCapacity_ShouldReturn422()
    {
        for (int i = 0; i < 3; i++)
        {
            var s = await _client.PostAsJsonAsync("/api/shipments", BuildCreateRequest(weight: 95));
            var sb = await s.Content.ReadFromJsonAsync<JsonElement>();
            var sid = DataString(sb, "id");
            await _client.PostAsJsonAsync($"/api/shipments/{sid}/assign",
                new { DriverId = DataSeeder.DriverMariaId, AssignedBy = "op1" });
        }

        var overflow = await _client.PostAsJsonAsync("/api/shipments", BuildCreateRequest(weight: 20));
        var overflowBody = await overflow.Content.ReadFromJsonAsync<JsonElement>();
        var overflowId = DataString(overflowBody, "id");

        var response = await _client.PostAsJsonAsync($"/api/shipments/{overflowId}/assign",
            new { DriverId = DataSeeder.DriverMariaId, AssignedBy = "op1" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task GetDriverMetrics_ShouldReturnAllDrivers()
    {
        var response = await _client.GetAsync("/api/drivers/metrics");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(3, Data(body).GetArrayLength());
    }
}
