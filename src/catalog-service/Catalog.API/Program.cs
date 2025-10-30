using Catalog.Application;
using Catalog.Application.Contracts;
using Catalog.Application.DTOs;
using Catalog.Domain.Enums;
using Catalog.Infrastructure;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DI
var cs = builder.Configuration.GetConnectionString("Default");
builder.Services.AddCatalogInfrastructure(cs);
builder.Services.AddCatalogApplication();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// auto-migrate cho demo
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    bool hasMigrations = false;
    try
    {
        hasMigrations = db.Database.GetAppliedMigrations().Any();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Cannot connect to DB: " + ex.Message);
    }

    if (!hasMigrations)
        db.Database.EnsureCreated();
}

// Health check endpoint, cached for 30 seconds
app.MapGet("/catalog/health", (HttpResponse response) =>
{
    response.Headers["Cache-Control"] = "public, max-age=30"; // cache 30 giây
    response.Headers["Expires"] = DateTime.UtcNow.AddSeconds(30).ToString("R");

    return Results.Ok(new { ok = true, svc = "catalog" });
});

// Get all products, cached for 60 seconds
app.MapGet("/catalog/products", async (IProductQueries query, HttpResponse response) =>
{
    response.Headers["Cache-Control"] = "public, max-age=60";
    response.Headers["Expires"] = DateTime.UtcNow.AddSeconds(60).ToString("R");

    var result = await query.GetAllAsync(); 
    return Results.Ok(result);
});

// Search products by sellerId
app.MapGet("/catalog/products/Id/{productId}", async (int productId, IProductQueries query, HttpResponse response) =>
{
    // Disable caching for seller-specific queries
    response.Headers["Cache-Control"] = "no-cache";

    var result = await query.SearchByProductIDAsync(productId);
    return Results.Ok(result);
});

// Seller search -> Show all products of a specific seller (by sellerId)
app.MapGet("/catalog/products/seller/{sellerId}", async (int sellerId, IProductQueries query, HttpResponse response) =>
{
    // Disable caching for seller-specific queries
    response.Headers["Cache-Control"] = "no-cache";

    var result = await query.SearchBySellerAsync(sellerId);
    return Results.Ok(result);
});

// Buyer search (Available only)
app.MapGet("/catalog/products/search", async (
    string? q,
    decimal? minPrice,
    decimal? maxPrice,
    string? pickupAddress,
    IProductQueries query,
    HttpResponse response) =>
{
    response.Headers["Cache-Control"] = "public, max-age=60";
    response.Headers["Expires"] = DateTime.UtcNow.AddSeconds(60).ToString("R");

    var result = await query.SearchWithFiltersAsync(q, minPrice, maxPrice, pickupAddress, ProductStatus.Available);
    return Results.Ok(result);
});

// Admin or Seller search (All statuses)
app.MapGet("/catalog/products/search/all", async (
    string? q,
    decimal? minPrice,
    decimal? maxPrice,
    string? pickupAddress,
    ProductStatus? status,
    IProductQueries query,
    HttpResponse response) =>
{
    response.Headers["Cache-Control"] = "no-cache";
    var result = await query.SearchWithFiltersAsync(q, minPrice, maxPrice, pickupAddress, status);
    return Results.Ok(result);
});

// Create new product (POST request) -> no caching
app.MapPost("/catalog/products", async (CreateProductReq req, IProductCommands cmd, HttpResponse response) =>
{
    response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
    response.Headers["Pragma"] = "no-cache";
    response.Headers["Expires"] = "0";

    var id = await cmd.CreateAsync(new CreateProductDto
    {
        Title = req.Title,
        Price = req.Price,
        SellerId = req.SellerId,
        PickupAddress = req.PickupAddress,
        ProductName = req.ProductName,
        Description = req.Description,
        RegistrationCard = req.RegistrationCard,
        FileUrl = req.FileUrl,
        ImageUrl = req.ImageUrl
    });

    return Results.Created($"/catalog/products/{id}", new { productId = id });
});

// Update product status (PATCH request) -> no caching
app.MapPatch("/catalog/products/status", async (UpdateProductStatusReq req, IProductCommands cmd) =>
{
    var ok = await cmd.UpdateStatusAsync(req.ProductId, req.NewStatus);
    if (!ok)
        return Results.NotFound(new { message = "Product not found" });

    return Results.Ok(new { message = "Status updated successfully" });
});


//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Request DTOs
record CreateProductReq(string Title, decimal Price, int SellerId, string PickupAddress,
    string ProductName, string Description, string? RegistrationCard, string? FileUrl, string? ImageUrl);

record UpdateProductStatusReq(int ProductId, ProductStatus NewStatus);