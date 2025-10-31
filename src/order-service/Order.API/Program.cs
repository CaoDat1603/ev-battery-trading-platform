using Order.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Order.Domain.Abstraction;
using Order.Infrastructure.Repositories;
using Order.Application.Contracts;
using Order.Application.Services;
using Order.Infrastructure;
using Order.Application;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ***************** DI *****************
var cs = builder.Configuration.GetConnectionString("Default");
builder.Services.AddOrderInfrastructure(cs); // Đăng ký DbContext và Repository
builder.Services.AddOrderApplication(); // Đăng ký Service nghiệp vụ
// **************************************

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --------------------------------------
// Tự động Migration (di chuyển DB)
// --------------------------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    bool hasMigration = false;
    try
    {
        // Kiểm tra xem có thể kết nối và lấy migrations không
        hasMigration = db.Database.GetAppliedMigrations().Any();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Cannot connect to Order DB: " + ex.Message);
    }

    // Nếu không có migrations, đảm bảo DB được tạo (chế độ EnsureCreated không khuyến khích, nhưng dễ cho Demo)
    if (!hasMigration)
    {
        db.Database.EnsureCreated();
    }
    else
    {
        // Nếu đã có migrations, áp dụng các migrations mới
        db.Database.Migrate();
    }
}
// --------------------------------------

// app.MapGet, app.MapPost, etc.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
