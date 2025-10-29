using Order.Application.Contracts;
using Order.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Order.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddOrderApplication(this IServiceCollection services)
        {
            // Đăng ký Service nghiệp vụ
            services.AddScoped<ITransactionService, TransactionService>();

            return services;
        }
    }
}
