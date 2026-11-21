using System.Text;
using ITHUB.SampleAPI.Api.Authentication;
using ITHUB.SampleAPI.Api.Infrastructure.Seeding;
using ITHUB.SampleAPI.Application.Services.Authentication;
using ITHUB.SampleAPI.Application.Services.Logging;
using ITHUB.SampleAPI.Application.Mapping;
using ITHUB.SampleAPI.Application.Services.Tasks;
using ITHUB.SampleAPI.Common.Constants;
using ITHUB.SampleAPI.Domain.Entities;
using ITHUB.SampleAPI.Persistence.Data;
using ITHUB.SampleAPI.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace ITHUB.SampleAPI.Api.Extensions;

public static class ApiExtensions
{
    public static IServiceCollection AddSampleApi(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = Environment.GetEnvironmentVariable(ApiConstants.DatabaseConnectionEnvironmentVariable)
            ?? configuration.GetConnectionString(ApiConstants.DatabaseConnectionName);
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("A SQL Server connection string must be configured.");

        services.AddDbContext<SampleApiDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IStatusRepository, StatusRepository>();
        services.AddScoped<IApplicationLogRepository, ApplicationLogRepository>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<ITokenIssuer, JwtTokenIssuer>();
        services.AddScoped<ILogService, LogService>();
        services.AddScoped<ITaskBoardService, TaskBoardService>();
        services.AddSingleton(SampleApiMappingConfiguration.Create());
        services.AddScoped<IPasswordHasher<SampleUserEntity>, PasswordHasher<SampleUserEntity>>();
        services.AddHttpContextAccessor();
        services.Configure<AdminSeedOptions>(configuration.GetSection("SeedData:Administrator"));
        services.Configure<UserSeedOptions>(configuration.GetSection("SeedData:User"));
        services.AddScoped<SampleApiSeedData>();
        services.AddHostedService<DatabaseSeedHostedService>();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(ApiConstants.JwtSection))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "Jwt:Issuer is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "Jwt:Audience is required.")
            .Validate(options => Encoding.UTF8.GetByteCount(options.SigningKey) >= 32,
                "Jwt:SigningKey must contain at least 32 UTF-8 bytes. Set it using user-secrets or an environment variable.")
            .Validate(options => options.ExpirationMinutes is > 0 and <= 1440, "Jwt:ExpirationMinutes must be between 1 and 1440.")
            .ValidateOnStart();

        var jwt = configuration.GetSection(ApiConstants.JwtSection).Get<JwtOptions>() ?? new JwtOptions();
        if (Encoding.UTF8.GetByteCount(jwt.SigningKey) < 32)
            throw new InvalidOperationException("Jwt:SigningKey must contain at least 32 UTF-8 bytes. Configure it with user-secrets before starting the API.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = System.Security.Claims.ClaimTypes.Name
                };
            });
        services.AddAuthorization();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "ITHUB SampleAPI", Version = "v1" });
            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header
            });
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, document)] = []
            });
        });

        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        services.AddCors(options => options.AddDefaultPolicy(policy =>
        {
            if (origins is { Length: > 0 })
                policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
            else
                policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }));

        return services;
    }
}
