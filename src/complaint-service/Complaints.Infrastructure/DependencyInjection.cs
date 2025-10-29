using Complaints.Domain.Abtractions;
using Complaints.Infrastructure.Repositories;
using Complaints.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Complaints.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddComplaintInfrastructure(this IServiceCollection services, string? connectionString)
        {
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
            services.AddScoped<IComplaintRepository, ComplaintRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ILocalFileStorage, LocalFileStorage>();
            services.AddScoped<IEvidenceHandler, EvidenceHandler>();
            return services;
        }
    }
}
