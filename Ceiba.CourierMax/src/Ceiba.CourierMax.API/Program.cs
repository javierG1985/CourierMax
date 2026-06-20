using Ceiba.CourierMax.API;
using Ceiba.CourierMax.API.Middleware;
using Ceiba.CourierMax.Application;
using Ceiba.CourierMax.Persistence.Context;
using Ceiba.CourierMax.Persistence;
using Ceiba.CourierMax.Persistence.Seed;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPersistence(builder.Configuration)
.AddApplicationServices()
.AddWebApi(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CourierMaxDbContext>();
    db.Database.EnsureCreated();
    await DataSeeder.SeedAsync(db);
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CourierMax API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
