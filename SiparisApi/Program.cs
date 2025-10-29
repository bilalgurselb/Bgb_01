//Bilal
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SiparisApi.Data;
using SiparisApi.Models;
using SiparisApi.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 📦 DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)
    ));

// 🌍 CORS
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("Default", p => p
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// 🧩 Dependency Injection
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>(); // sadece bir kez tanımlı

// 🔐 JWT Authentication
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty))
        };
    });

builder.Services.AddAuthorization();

// 📘 MVC + Swagger
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddSession();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 🚦 Middleware Pipeline
app.UseStaticFiles();
app.UseSession();
app.UseCors("Default");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapDefaultControllerRoute();
// ✅ Varsayılan yönlendirme: / isteği Login'e gitsin
app.MapGet("/", context =>
{
    context.Response.Redirect("/Account/Login");
    return Task.CompletedTask;
});

// 🌡️ Warm-up 
Task.Run(async () =>
{
    await Task.Delay(2000);
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // 🔸 Veritabanı bağlantısını test et
        await db.Database.CanConnectAsync();

        // 🔹 Eğer hiç AllowedEmail kaydı yoksa, ilk admini ekle
        if (!db.AllowedEmails.Any())
        {
            db.AllowedEmails.Add(new AllowedEmail
            {
                Email = "bborekci@sintankimya.com",
                Role = "Admin",
                IsActive = true
            });
            await db.SaveChangesAsync();
        }

        // 🔹 Ledger nedeniyle null Role kayıtlarını düzelt
        var emptyRoles = db.AllowedEmails.Where(x => x.Role == null).ToList();
        if (emptyRoles.Any())
        {
            foreach (var rec in emptyRoles)
                rec.Role = "User";
            await db.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Warm-up] {ex.Message}");
    }
});

app.Run();
