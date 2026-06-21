using Ceiba.CourierMax.Application.Interfaces;
using Ceiba.CourierMax.Application.Mappers;
using Ceiba.CourierMax.Application.Services;
using Ceiba.CourierMax.Application.UseCases.Auth;
using Ceiba.CourierMax.Application.UseCases.Reports.GetDelayedShipments;
using Ceiba.CourierMax.Application.UseCases.Reports.GetDriverMetrics;
using Ceiba.CourierMax.Application.UseCases.Shipments.AssignShipment;
using Ceiba.CourierMax.Application.UseCases.Shipments.CancelShipment;
using Ceiba.CourierMax.Application.UseCases.Shipments.ChangeShipmentStatus;
using Ceiba.CourierMax.Application.UseCases.Shipments.CreateShipment;
using Ceiba.CourierMax.Application.UseCases.Shipments.GetShipment;
using Ceiba.CourierMax.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Ceiba.CourierMax.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Auth
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ILoginUseCase, LoginUseCase>();

        // Shipments
        services.AddScoped<ICreateShipmentUseCase, CreateShipmentUseCase>();
        services.AddScoped<IGetShipmentUseCase, GetShipmentUseCase>();
        services.AddScoped<IAssignShipmentUseCase, AssignShipmentUseCase>();
        services.AddScoped<IChangeShipmentStatusUseCase, ChangeShipmentStatusUseCase>();
        services.AddScoped<ICancelShipmentUseCase, CancelShipmentUseCase>();

        // Reports
        services.AddScoped<IGetDriverMetricsUseCase, GetDriverMetricsUseCase>();
        services.AddScoped<IGetDelayedShipmentsUseCase, GetDelayedShipmentsUseCase>();

        // Validadores FluentValidation — registra todos los validators del assembly Application
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

        // AutoMapper — registra MappingProfile desde el assembly Application
        services.AddAutoMapper(_ => { }, typeof(MappingProfile));

        return services;
    }
}
