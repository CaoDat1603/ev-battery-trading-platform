using Payment.Domain.Abstraction;
using Payment.Infrastructure.Data;
using Payment.Infrastructure.Gateways;
using Payment.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application.Contracts;
using Payment.Infrastructure.Clients;
using Microsoft.Extensions.Configuration;

namespace Payment.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPaymentInfrastructure(this IServiceCollection services, string connectionString, IConfiguration configuration)
        {
            // 1. Đăng ký DbContext
            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

            // 2. Đăng ký Repository
            services.AddScoped<IPaymentRepository, PaymentRepository>();

            // 3. Đăng ký External Service (VNPAY Gateway)
            services.AddScoped<IVnPayService, VnPayService>();
            services.AddHttpClient<IInternalTokenService, InternalTokenService>();

            // 4. Đăng ký HttpClient Factory cho Order Service Client
            var orderApiBaseUrl = configuration["Clients:OrderApiBaseUrl"] ?? "http://order-api:8080";
            services.AddHttpClient<IOrderServiceClient, OrderServiceClient>(client =>
            {
                client.BaseAddress = new Uri(orderApiBaseUrl);

                client.DefaultRequestHeaders.Add("x-internal-key", configuration["InternalApiKey"]);
            });

            return services;
        }
    }
}
