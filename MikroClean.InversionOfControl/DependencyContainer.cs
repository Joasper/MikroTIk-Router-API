
using Microsoft.Extensions.DependencyInjection;
using MikroClean.Application.Interfaces;
using MikroClean.Application.Services;
using MikroClean.Domain.Interfaces;
using MikroClean.Domain.Interfaces.UOW;
using MikroClean.Infrastructure.Repositories;
using MikroClean.Infrastructure.Repositories.UOW;

namespace MikroClean.InversionOfControl
{
    public static class DependencyContainer
    {
        public static IServiceCollection AddDependencies(this IServiceCollection services)
        {

            services.AddScoped<IDeviceService, DeviceService>();

            services.AddScoped<IDeviceRepository, DeviceRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            return services;
        }
    }
}
