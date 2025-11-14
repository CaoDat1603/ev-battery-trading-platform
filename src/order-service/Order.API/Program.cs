using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Order.Application;
using Order.Infrastructure;
using Order.Infrastructure.Data;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization; // Added for [Authorize]

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// --- 1. Cấu hình Swagger (Thêm nút Authorize) ---
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Order API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' + space + your token. Example: 'Bearer 12345'"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// --- 2. Cấu hình JWT Authentication ---
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
builder.Services.AddAuthorization(options =>
{
    // Policy cho service nội bộ
    options.AddPolicy("SystemPolicy", policy =>
    {
        policy.AuthenticationSchemes.Add("SystemBearer");
        policy.RequireRole("System");
    });
});

// --- 4. Đăng ký DI (DI) ---
var cs = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(cs))
    throw new InvalidOperationException("Database connection string 'Default' is not configured.");

builder.Services.AddOrderInfrastructure(cs, builder.Configuration);
builder.Services.AddOrderApplication();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --- 5. Tự động Migration (Migration) ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

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


app.UseAuthentication();
app.UseAuthorization();

// Minimal API debug claims
app.MapGet("/api/auth/me", [Authorize] (ClaimsPrincipal user) =>
{
    var all = user.Claims.Select(c => new { c.Type, c.Value });
    var roles = user.Claims
                    .Where(x => x.Type == "role" || x.Type.EndsWith("/role"))
                    .Select(x => x.Value);
    var name = user.FindFirst("unique_name")?.Value
                ?? user.FindFirst(ClaimTypes.Name)?.Value;

    return Results.Ok(new { name, roles, all });
});

app.Use(async (ctx, next) =>
{
    try { await next(); }
    catch (Exception ex)
    {
        Console.WriteLine("FEES ERROR: " + ex);
        ctx.Response.StatusCode = 500;
        await ctx.Response.WriteAsync("ERR: " + ex.Message);
    }
});


app.MapControllers();
app.Run();
