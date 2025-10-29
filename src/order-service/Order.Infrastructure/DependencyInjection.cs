using Order.Domain.Abstraction;
using Order.Infrastructure.Data;
using Order.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Order.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddOrderInfrastructure(this IServiceCollection services, string ConnectionString)
        {
            // 1. Đăng ký DbContext (Kết nối PostgreSQL)
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(ConnectionString));

            // 2. Đăng ký Repository (Abstraction -> Implementation)
            services.AddScoped<ITransactionRepository, TransactionRepository>();

            return services;
        }
    }
}
