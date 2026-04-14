using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using LostAndFound.Core.Interfaces;
using LostAndFound.Infrastructure.Interceptors;
using LostAndFound.Infrastructure.Services;

namespace LostAndFound.Infrastructure.Repository;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // ── Database ─────────────────────────────────────────────────────────
        services.AddSingleton<AuditInterceptor>();
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
        });

        // ── Unit of Work & Repositories ───────────────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IFileService, FileService>();

        // ── ASP.NET Core Identity ─────────────────────────────────────────────
        services.AddIdentity<User, Role>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // ── JWT Options (Options Pattern) ─────────────────────────────────────
        // Validates that the section exists and the secret key is strong enough at startup.
        var jwtSection = configuration.GetSection("JwtOptions");
        var secretKey = jwtSection["SecretKey"]
            ?? throw new InvalidOperationException("JwtOptions:SecretKey is missing from configuration.");

        if (Encoding.UTF8.GetByteCount(secretKey) < 32)
            throw new InvalidOperationException("JwtOptions:SecretKey must be at least 256 bits (32 UTF-8 bytes) long.");

        services.Configure<JwtOptions>(jwtSection);
        services.AddScoped<IJwtProvider, JwtProvider>();

        // ── JWT Bearer Authentication ─────────────────────────────────────────
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSection["Issuer"],
                ValidAudience = jwtSection["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            }
        );

        // ── Firebase Initialization (Notifications, Auth, etc) ───────────────
        if (FirebaseApp.DefaultInstance == null)
        {
            var serviceAccountPath = configuration["Firebase:ServiceAccountPath"]
                ?? throw new InvalidOperationException(
                    "Firebase:ServiceAccountPath is missing. Set it via an environment variable or user secrets.");

            // Path is relative to the directory where the application runs
            var fullPath = Path.Combine(AppContext.BaseDirectory, serviceAccountPath);
            if (!File.Exists(fullPath))
            {
                // Fallback for development where it runs from project root
                fullPath = serviceAccountPath;
            }

            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(fullPath)
            });
        }

        // ── Authentication Strategy Selection ────────────────────────────────
        var authProvider = configuration["AuthenticationSettings:Provider"];

        if (authProvider == "Firebase")
        {
            // Initialize the Firebase Admin SDK exactly once.
            // The service account key path must come from an environment variable or secrets manager,
            // NOT from appsettings.json checked into source control.
            if (FirebaseApp.DefaultInstance == null)
            {
                var serviceAccountPath = configuration["Firebase:ServiceAccountPath"]
                    ?? throw new InvalidOperationException(
                        "Firebase:ServiceAccountPath is missing. Set it via an environment variable or user secrets.");

                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(serviceAccountPath)
                });
            }

            services.AddScoped<IAuthenticationService, FirebaseAuthService>();
        }
        else
        {
            services.AddScoped<IAuthenticationService, IdentityAuthService>();
        }

        return services;
    }
}