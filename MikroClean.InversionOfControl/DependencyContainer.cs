using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MikroClean.Application.Interfaces;
using MikroClean.Application.Services;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Domain.Interfaces.Security;
using MikroClean.Domain.Interfaces.UOW;
using MikroClean.Domain.MikroTik;
using MikroClean.Infrastructure.Context;
using MikroClean.Infrastructure.MikroTik;
using MikroClean.Infrastructure.Repositories;
using MikroClean.Infrastructure.Repositories.UOW;
using MikroClean.Infrastructure.Security;
using System.Text;

namespace MikroClean.InversionOfControl
{
    public static class DependencyContainer
    {
        public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            
            // Repositories
            services.AddScoped<IOrganizationRepository, OrganizationRepository>();
            services.AddScoped<IRouterRepository, RouterRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ILicenseRepository, LicenseRepository>();
            services.AddScoped<ISystemRoleRepository, SystemRoleRepository>();
            
            // Application Services
            services.AddScoped<IOrganizationService, OrganizationService>();
            services.AddScoped<IRouterService, RouterService>();
            services.AddScoped<IMikroTikService, MikroTikService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ILicenseService, LicenseService>();
            services.AddScoped<ISystemRoleService, SystemRoleService>();
            services.AddScoped<IAuthService, AuthService>();
            
            // Security Services
            services.AddSingleton<IEncryptionService, AesEncryptionService>();
            services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            
            // MikroTik Infrastructure (Singleton para mantener el pool de conexiones)
            services.AddSingleton<IMikroTikClientFactory, MikroTikClientFactory>();
            services.AddSingleton<IMikroTikConnectionManager, MikroTikConnectionManager>();
            
            // MikroTik Retry Policy Configuration desde appsettings
            services.AddSingleton(sp =>
            {
                var retrySection = configuration.GetSection("MikroTik:RetryPolicy");
                return new MikroTikRetryPolicy
                {
                    MaxRetryAttempts = retrySection.GetValue("MaxRetryAttempts", 3),
                    InitialDelay = TimeSpan.FromSeconds(retrySection.GetValue("InitialDelaySeconds", 1)),
                    MaxDelay = TimeSpan.FromSeconds(retrySection.GetValue("MaxDelaySeconds", 10)),
                    BackoffMultiplier = retrySection.GetValue("BackoffMultiplier", 2.0)
                };
            });
            
            // Memory Cache para status de routers y caché de conexiones
            services.AddMemoryCache();
            
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

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key no configurada");
            var key = Encoding.UTF8.GetBytes(jwtKey);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            return services;
        }
    }
}
