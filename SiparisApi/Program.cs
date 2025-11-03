// Bilal
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
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
builder.Services.AddScoped<IEmailService, EmailService>();

// 🔐 JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
    throw new InvalidOperationException("JWT anahtarı (Jwt:Key) appsettings.json veya ortam değişkeninde tanımlı olmalı.");

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    })
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// 📘 MVC + Swagger + Session
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
// ✅ Azure App Service'de HTTPS algılaması için ForwardedHeaders kullan
var forwardedHeaderOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
app.UseForwardedHeaders(forwardedHeaderOptions);


// 🚦 Middleware Pipeline
app.UseStaticFiles();
app.UseSession();
app.UseCors("Default");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// 🚫 Login olmadan doğrudan erişimi engelle
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower();
    var token = context.Session.GetString("AccessToken");

    // API veya yönetim ekranlarına doğrudan erişim denemelerini engelle
    if (string.IsNullOrEmpty(token) &&
        (path!.StartsWith("/api/") || path!.StartsWith("/admin") || path!.StartsWith("/ordersui") || path!.StartsWith("/ordersuilist")))
    {
        if (!path.StartsWith("/account/login") && !path.StartsWith("/api/auth"))
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "text/html; charset=utf-8";

            var html = @"
<!DOCTYPE html>
<html lang='tr'>
<head>
<meta charset='utf-8'>
<title>Erişim Engellendi - SINTAN CHEMICALS</title>
<style>
body {
    background-color: #f8f9fa;
    font-family: 'Inter', sans-serif;
    color: #333;
    display: flex;
    justify-content: center;
    align-items: center;
    height: 100vh;
}
.card {
    background: white;
    border-radius: 16px;
    box-shadow: 0 4px 20px rgba(0,0,0,0.1);
    padding: 40px;
    text-align: center;
    max-width: 400px;
}
h1 {
    color: #a81e24;
    font-size: 22px;
    margin-bottom: 12px;
}
p {
    color: #555;
    font-size: 14px;
}
button {
    margin-top: 20px;
    background-color: #a81e24;
    color: #fff;
    border: none;
    padding: 10px 22px;
    border-radius: 6px;
    cursor: pointer;
}
button:hover {
    background-color: #8f1a1f;
}
</style>
</head>
<body>
    <div class='card'>
        <h1>Erişim Engellendi</h1>
        <p>Bu sayfayı görüntüleme izniniz bulunmamaktadır.</p>
        <form action='/Account/Login' method='get'>
            <button type='submit'>Giriş Ekranına Dön</button>
        </form>
    </div>
</body>
</html>";

            await context.Response.WriteAsync(html);
            return;
        }
    }

    await next();
});

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
#pragma warning disable CS4014
Task.Run(async () =>
{
    await Task.Delay(2000);    
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.CanConnectAsync();
        {
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
                Console.WriteLine("[Warm-up] İlk admin eklendi: bborekci@sintankimya.com");
            }
            // 🔹 Ledger nedeniyle null Role kayıtlarını düzelt
            var emptyRoles = db.AllowedEmails.Where(x => x.Role == null).ToList();
            if (emptyRoles.Any())
            {
                foreach (var rec in emptyRoles)
                    rec.Role = "User";
                await db.SaveChangesAsync();
                Console.WriteLine($"[Warm-up] {emptyRoles.Count} kullanıcıda eksik rol düzeltildi.");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Warm-up] {ex.Message}");
    }
});
#pragma warning restore CS4014
app.Run();
