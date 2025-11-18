// Bilal
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using SiparisApi.Data;
using SiparisApi.Models;
using SiparisApi.Services;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Cookie.Name = "AuthCookie";
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
    })
     .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
      options.RequireHttpsMetadata = false;
      options.SaveToken = true;
      options.TokenValidationParameters = new TokenValidationParameters
      {
          ValidateIssuer = false,
          ValidateAudience = false,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        
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

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseRouting();
app.Use(async (context, next) =>
{
    if (!context.Request.Headers.ContainsKey("Authorization") &&
        context.Request.Cookies.TryGetValue("AccessToken", out var token))
    {
        context.Request.Headers.Append("Authorization", $"Bearer {token}");
    }

    await next();
});
app.UseSession();
app.UseCors("Default");
app.UseAuthentication();
app.UseAuthorization();


app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower();
    var token = context.Session.GetString("AccessToken");

    if (string.IsNullOrEmpty(token) &&
        (path!.StartsWith("/api/") || path!.StartsWith("/admin") || path!.StartsWith("/ordersui") || path!.StartsWith("/ordersuilist")))
    {
        if (!path.StartsWith("/account/login") && !path.StartsWith("/api/auth"))
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "text/html; charset=utf-8";

            var html = @"<!DOCTYPE html>
<html lang='tr'>
<head>
<meta charset='utf-8'>
<title>Erişim Engellendi - SINTAN CHEMICALS</title>
<style>/* stil burada */</style>
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

app.MapGet("/", context =>
{
    context.Response.Redirect("/Account/Login");
    return Task.CompletedTask;
});

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
