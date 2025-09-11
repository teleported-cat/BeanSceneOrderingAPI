
namespace BeanSceneOrderingAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Get database details from appsettings.js
            builder.Services.Configure<BeanSceneDatabaseSettings>(builder.Configuration.GetSection("BeanSceneDatabaseSettings"));

            // Add CORS policy - Cross-Origin Resource Sharing
            builder.Services.AddCors(o => o.AddPolicy("BeanSceneCorsPolicy", builder => {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            }));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Enable CORS policy
            app.UseCors("BeanSceneCorsPolicy");

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
