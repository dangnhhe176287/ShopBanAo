
using EcommerceBackend.DataAccess;
using EcommerceBackend.DataAccess.Models;
using EcommerceBackend.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.API
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add DbContext
            services.AddDbContext<EcommerceDBContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Add Repositories
            services.AddScoped<IProductRepository, ProductRepository>();

            // Add Services
         

            return services;
        }
    }
} 