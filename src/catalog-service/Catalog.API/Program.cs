using Catalog.Application;
using Catalog.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// === Add services ===
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// === Dependency Injection ===
var cs = builder.Configuration.GetConnectionString("Default");
builder.Services.AddCatalogInfrastructure(cs);
builder.Services.AddCatalogApplication();

var app = builder.Build();

// === Configure middleware ===
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// === Auto-migrate (demo only) ===
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        if (!db.Database.GetAppliedMigrations().Any())
            db.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"DB connection error: {ex.Message}");
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();
