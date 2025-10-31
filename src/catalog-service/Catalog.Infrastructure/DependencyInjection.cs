﻿using Catalog.Domain.Abstractions;
using Catalog.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCatalogInfrastructure(this IServiceCollection services, string? connectionString)
        {
            services.AddDbContext<AppDbContext>(o => o.UseNpgsql(connectionString));
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ILocalFileStorage, Services.LocalFileStorage>();
            services.AddScoped<IProductImageHandler, Services.ProductImageHandler>();
            services.AddScoped<IProductFileHandler, Services.ProductFileHandler>();
            return services;
        }
    }
}
