using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestWebApi.Shared.Repositories;

namespace TestWebApi.Shared.Extensions
{
    public static class ApplicationServiceExtension
    {
        /// <summary>
        /// Registers open-generic repositories.
        /// </summary>
        public static IServiceCollection AddSharedRepositories(this IServiceCollection services)
        {
            // Register the open generic so any IGenericRepository<T> can be resolved.
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            return services;
        }

        /// <summary>
        /// Overload preserved in case code elsewhere passes IConfiguration (ignored).
        /// </summary>
        public static IServiceCollection AddSharedRepositories(this IServiceCollection services, IConfiguration _)
            => services.AddSharedRepositories();

        /// <summary>
        /// Registers domain/application services.
        /// </summary>
        public static IServiceCollection AddSharedServices(this IServiceCollection services)
        {
            services.AddScoped<Services.IProductService, Services.ProductService>();
            services.AddScoped<Services.ICategoryService, Services.CategoryService>();
            return services;
        }

        /// <summary>
        /// Overload to match calls that pass IConfiguration (if any).
        /// </summary>
        public static IServiceCollection AddSharedServices(this IServiceCollection services, IConfiguration _)
            => services.AddSharedServices();
    }
}