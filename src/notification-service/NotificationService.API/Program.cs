using Microsoft.EntityFrameworkCore;
using NotificationService.Application;
using NotificationService.Infrastructure;
using NotificationService.Infrastructure.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddNotificationInfrastructure(builder.Configuration, builder.Configuration.GetConnectionString("Default"));
builder.Services.AddNotificationApplication();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapHub<NotificationHub>("/hubs/notifications");
app.UseHttpsRedirection();
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

app.UseAuthorization();

app.MapControllers();

app.Run();
