using LogiTrack.WebApi.Options;
using LogiTrack.WebApi.Services;
using LogiTrack.WebApi.Repositories.Shipments;
using Microsoft.Extensions.Options;

namespace LogiTrack.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // MVC + Swagger
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Options
            builder.Services.Configure<LogisticsOptions>(builder.Configuration.GetSection("Logistics"));
            builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));

            // Services
            builder.Services.AddScoped<IDeliveryTimeService, DeliveryTimeService>();

            // Repositories: switch File - InMemory via config
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
