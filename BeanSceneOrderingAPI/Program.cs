
using BeanSceneOrderingAPI.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;

namespace BeanSceneOrderingAPI
{
    /// <summary>
    /// Class of the API's entry point.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The API's entry point.
        /// </summary>
        /// <param name="args">Unused. This program doesn't take any arguments.</param>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // Adds basic endpoint authorisation
            builder.Services.AddAuthentication("BasicAuthentication")
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
            builder.Services.AddAuthorization();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "BeanSceneOrderingAPI",
                    Version = "v1"
                });

                options.AddSecurityDefinition("basic", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "basic",
                    In = ParameterLocation.Header,
                    Description = "Basic Authentication header using the Bearer scheme."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "basic"
                            }
                        },
                        new string[] {}
                    }
                });
            });

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
