using LogiTrack.WebApi.Data;
using LogiTrack.WebApi.Models;
using LogiTrack.WebApi.Options;
using LogiTrack.WebApi.Repositories;
using LogiTrack.WebApi.Repositories.EF;
using LogiTrack.WebApi.Seed;
using LogiTrack.WebApi.Services;
using LogiTrack.WebApi.Services.Abstractions;
using LogiTrack.WebApi.Services.Factories;
using LogiTrack.WebApi.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;

namespace LogiTrack.WebApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File(
                    path: "Logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    shared: true
                )
                .CreateLogger();

            try
            {
                var builder = WebApplication.CreateBuilder(args);

                builder.Host.UseSerilog();

                builder.Services
                    .AddControllers()
                    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LogiTrack API", Version = "v1" });
                    var jwtScheme = new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "Bearer {token}"
                    };
                    c.AddSecurityDefinition("Bearer", jwtScheme);
                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        { jwtScheme, Array.Empty<string>() }
                    });
                });

                builder.Services.Configure<LogisticsOptions>(builder.Configuration.GetSection("Logistics"));
                builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));

                builder.Services.AddDbContext<LogiTrackDbContext>(options =>
                {
                    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"));
                });

                builder.Services
                    .AddIdentityCore<ApplicationUser>(opt =>
                    {
                        opt.Password.RequireDigit = true;
                        opt.Password.RequireUppercase = false;
                        opt.Password.RequireNonAlphanumeric = false;
                        opt.Password.RequiredLength = 6;
                        opt.User.RequireUniqueEmail = true;
                    })
                    .AddRoles<IdentityRole>()
                    .AddEntityFrameworkStores<LogiTrackDbContext>()
                    .AddDefaultTokenProviders();

                builder.Services.AddDataProtection();

                var jwt = builder.Configuration.GetSection("Jwt");
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

                builder.Services
                    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(opts =>
                    {
                        opts.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer = jwt["Issuer"],
                            ValidateAudience = true,
                            ValidAudience = jwt["Audience"],
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = key,
                            ValidateLifetime = true,
                            ClockSkew = TimeSpan.FromSeconds(30)
                        };
                    });

                builder.Services.AddAuthorization();

                builder.Services.AddSingleton<IApiMetricsService, InMemoryApiMetricsService>();

                builder.Services.AddScoped<ITokenService, TokenService>();

                builder.Services.AddScoped<IShipmentsRepository, ShipmentsRepository>();

                /*
                // переключення файл/пам'ять
                builder.Services.AddSingleton<IShipmentsRepository>(sp =>
                {
                    var env = sp.GetRequiredService<IWebHostEnvironment>();
                    var storage = sp.GetRequiredService<IOptions<StorageOptions>>().Value;

                    var fullPath = Path.IsPathRooted(storage.ShipmentsFilePath)
                        ? storage.ShipmentsFilePath
                        : Path.Combine(env.ContentRootPath, storage.ShipmentsFilePath);

                    return new FileShipmentsRepository(fullPath);
                });
                */

                builder.Services.AddScoped<ICustomersRepository, CustomersRepository>();
                builder.Services.AddScoped<IDriversRepository, DriversRepository>();
                builder.Services.AddScoped<IVehiclesRepository, VehiclesRepository>();
                builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
                builder.Services.AddSingleton<DeliveryTimeServiceFactory>();

                var app = builder.Build();

                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    await SeedData.InitializeAsync(services);
                }

                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();

                app.UseAuthentication();

                app.UseMiddleware<RequestLoggingMiddleware>();

                app.UseAuthorization();

                app.MapControllers();

                await app.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
