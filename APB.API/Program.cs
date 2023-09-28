using ABP.Application.Interfaces;
using ABP.Application.Services;
using ABP.Domain.Repository;
using ABP.Infrastructure.Repository;

namespace APB.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            ConfigurationManager configuration = builder.Configuration;

            builder.Services.AddSingleton<IDeviceRepository, DeviceRepository>(sp => {
                return new DeviceRepository(configuration.GetConnectionString("DefaultConnection"));
            });
            builder.Services.AddSingleton<IExperimentRepository, ExperimentRepository>(sp => {
                return new ExperimentRepository(configuration.GetConnectionString("DefaultConnection"));
            });
            builder.Services.AddSingleton<IChanceBasedOutputService, ChanceBasedOutputService>();
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
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