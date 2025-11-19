using Complaints.Domain.Abtractions;
using Complaints.Infrastructure.Repositories;
using Complaints.Infrastructure.Services;
using Complaints.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Complaints.Infrastructure.Messaging;

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
            services.AddHttpClient<IInternalTokenService, InternalTokenService>();
            services.AddHttpClient<IIdentityClient, IdentityClient>();
            services.AddHttpClient<IOrderClient, OrderClient >();
            services.AddHttpClient<ICatalogClient, CatalogClient>();
            services.AddSingleton<IEventBus, ComplaintPublisher>();
            return services;
        }
    }
}
