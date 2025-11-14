using Payment.Application.Contracts;
using Payment.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Payment.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPaymentApplication(this IServiceCollection services)
        {
            services.AddScoped<IPaymentService, PaymentService>();

            return services;
        }
    }
}
