using Complaints.Application.Contracts;
using Complaints.Application.Services;

namespace Complaints.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddComplaintAplication(this IServiceCollection services)
        {
            services.AddScoped<IComplaintCommands, ComplaintCommands>();
            services.AddScoped<IComplaintQueries, ComplaintQueries>();
            return services;
        }
    }
}
