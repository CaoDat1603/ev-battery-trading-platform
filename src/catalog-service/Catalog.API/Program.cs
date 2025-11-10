using Catalog.Application;
using Catalog.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// === Add services ===
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// === Dependency Injection ===
var cs = builder.Configuration.GetConnectionString("Default");
builder.Services.AddCatalogInfrastructure(cs);
builder.Services.AddCatalogApplication();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer("Bearer", opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    })
    .AddJwtBearer("SystemBearer", opt =>
    {
        // Token của service nội bộ
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSystem:Issuer"],
            ValidAudience = builder.Configuration["JwtSystem:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSystem:SigningKey"]!))
        };
    });
/*builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080); // http
    options.ListenAnyIP(8081, o => o.UseHttps()); // https
});*/

builder.Services.AddAuthorization(options =>
{
    // Policy cho service nội bộ
    options.AddPolicy("SystemPolicy", policy =>
    {
        policy.AuthenticationSchemes.Add("SystemBearer");
        policy.RequireRole("System");
    });
});
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
