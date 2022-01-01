using IPclient.Services;
using IPClient.Shared.IServices;
using IPClient.Shared.ModelDto;
using IPClient.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IPclient.Extensions
{
    public static class ServiceCollections
    {
        //
        //  Summary:
        //      Register DI when run console.
        public static IServiceCollection AddServiceCollections(this IServiceCollection services, IConfiguration configuration)
        {
            var configure = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();
            //inject services here...
            services.AddTransient<IConfigurationService, ConfigurationService>();
            services.AddTransient<IPingService, PingService>();
            services.AddTransient<IApp, App>();
            services.Configure<ApiConfiguration>(configure.GetSection("ApiConnections:Default"));
            services.AddScoped(cfg => cfg.GetService<IOptions<ApiConfiguration>>().Value);
            //inject features
            services.AddServices();

            return services;
        }

        //here auto register DI implement from IService
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            var iService = typeof(IServiceCollection);

            var types = iService
                .Assembly
                .GetExportedTypes()
                .Where(t => iService.IsAssignableFrom(t) && t.Name != iService.Name) //select services implement from IService
                .Select(t => new
                {
                    InterfaceService = t.GetInterface($"I{t.Name}"),
                    Service = t.Name,
                    Implementation = t
                })
                .Where(t => t.Service != null);

            foreach (var type in types)
            {
                if (type.InterfaceService != null)
                {
                    services.AddTransient(type.InterfaceService, type.Implementation);
                }
                else
                {
                    if (!type.Implementation.IsInterface)
                    {
                        services.AddTransient(type.Implementation);
                    }
                }
            }

            return services;
        }
    }
}