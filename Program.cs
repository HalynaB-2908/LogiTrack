using LogiTrack.WebApi.Data;
using LogiTrack.WebApi.Options;
using LogiTrack.WebApi.Repositories.Shipments;
using LogiTrack.WebApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;

namespace LogiTrack.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services
                .AddControllers()
                .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.Configure<LogisticsOptions>(builder.Configuration.GetSection("Logistics"));
            builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));

            builder.Services.AddDbContext<LogiTrackDbContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"));
            });

            builder.Services.AddScoped<IDeliveryTimeService, DeliveryTimeService>();

            builder.Services.AddSingleton<IShipmentsRepository>(sp =>
            {
                var env = sp.GetRequiredService<IWebHostEnvironment>();
                var storage = sp.GetRequiredService<IOptions<StorageOptions>>().Value;

                if (storage.UseFileStorage)
                {
                    var fullPath = Path.IsPathRooted(storage.ShipmentsFilePath)
                        ? storage.ShipmentsFilePath
                        : Path.Combine(env.ContentRootPath, storage.ShipmentsFilePath);

                    return new FileShipmentsRepository(fullPath);
                }

                return new InMemoryShipmentsRepository();
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
