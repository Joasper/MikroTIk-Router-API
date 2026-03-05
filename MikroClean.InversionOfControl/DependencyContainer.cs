using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MikroClean.Application.Interfaces;
using MikroClean.Application.Services;
using MikroClean.Domain.Interfaces.Repositories;
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
            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            
            // Repositories
            services.AddScoped<IOrganizationRepository, OrganizationRepository>();
            
            // Services
            services.AddScoped<IOrganizationService, OrganizationService>();
            
            return services;
        }

        public static IServiceCollection AddMikroCleanContext(this IServiceCollection services, IConfiguration configuration) 
        {
            var connectionString = configuration.GetConnectionString("Connection");
            services.AddDbContext<MikroCleanContext>(options => options.UseSqlServer(connectionString));
            return services;
        }
        
        public static IServiceCollection AutomaticMigrate(this IServiceCollection services)
        {
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
