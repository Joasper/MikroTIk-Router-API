
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MikroClean.Application.Interfaces;
using MikroClean.Application.Services;
using MikroClean.Domain.Interfaces;
using MikroClean.Domain.Interfaces.UOW;
using MikroClean.Infrastructure.Context;
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

        public static IServiceCollection AddMikroCleanContext(this IServiceCollection services, IConfiguration configuration) 
        {
            // Get connection string from configuration
            var connectionString = configuration.GetConnectionString("Connection");

            // Add DbContext
            services.AddDbContext<MikroCleanContext>(options => options.UseSqlServer(connectionString));

            return services;
        }
        public static IServiceCollection AutomaticMigrate(this IServiceCollection services)
        {
           
            // Build a temporary service provider to get the DbContext
            var serviceProvider = services.BuildServiceProvider();
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<MikroCleanContext>();
                context.Database.EnsureCreated();
                context.Database.MigrateAsync();
            }
            return services;
        }
    }
}
