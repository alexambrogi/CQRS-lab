using CQRS.POC.Application.Abstractions;
using CQRS.POC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CQRS.POC.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddDbContext<AppDbContext>(options =>
            //    options.UseSqlite(configuration.GetConnectionString("DefaultConnection")
            //        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));

            services.AddDbContext<AppDbContext>(opt => opt.UseSqlite("Data Source=cqrs-poc.db"));
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductReadRepository, ProductReadRepository>();

            return services;
        }
    }
}
