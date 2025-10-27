// SSH push test - Bilal 2025-10-27

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SiparisApi.Data;
using SiparisApi.Models;
using SiparisApi.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)
    ));

// CORS
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("Default", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// DI
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>(); // ✅ MAIL SERVISI

// JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization();

// MVC + Swagger
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddSession();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

// Pipeline
app.UseSession();
app.UseCors("Default");
app.UseHttpsRedirection();

app.UseAuthentication();   // ✅ ÖNCE kimlik
app.UseAuthorization();    // ✅ SONRA yetki

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapDefaultControllerRoute();

// Warm-up (opsiyonel)
Task.Run(async () =>
{
    await Task.Delay(2000);
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (!db.AllowedEmails.Any())
        {
            db.AllowedEmails.AddRange(
                new AllowedEmail { Email = "admin@sintan.com" },
                new AllowedEmail { Email = "bilal@sintan.com" }
            );
            await db.SaveChangesAsync();
        }
    }
    catch (Exception ex) { Console.WriteLine($"[Warm-up] {ex.Message}"); }
});

app.Run();
