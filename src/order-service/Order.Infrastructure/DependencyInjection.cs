using Order.Domain.Abstraction;
using Order.Infrastructure.Data;
using Order.Infrastructure.Repositories;
using Order.Application.Contracts;
using Order.Infrastructure.Clients;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Order.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddOrderInfrastructure(this IServiceCollection services, string connectionString, IConfiguration configuration)
        {
            // 1. Đăng ký DbContext (Kết nối PostgreSQL)
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));

            // 2. Đăng ký Repository (Abstraction -> Implementation)
            services.AddScoped<ITransactionRepository, TransactionRepository>();

            // 3. Đăng ký HTTP Client (IPaymentServiceClient)
            string paymentApiUrl = configuration["Clients:PaymentApiBaseUrl"]
                ?? "http://payment-api:8080";

            services.AddHttpClient<IPaymentServiceClient, PaymentServiceClient>(client =>
            {
                client.BaseAddress = new Uri(paymentApiUrl);
            });

            return services;
        }
    }
}
