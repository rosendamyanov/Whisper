using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Whisper.Authentication.Configuration;
using Whisper.Authentication.Data;
using Whisper.Authentication.Data.Interfaces;
using Whisper.Authentication.Factory;
using Whisper.Authentication.Factory.Interfaces;
using Whisper.Authentication.Services;
using Whisper.Authentication.Services.Interfaces;
using Whisper.Authentication.Validation;
using Whisper.Authentication.Validation.Interfaces;
using Whisper.Data.Context;

namespace Whisper.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure DbContext
            builder.Services.AddDbContext<ApplicationContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

            // Add services to the container.
            // Repositories
            builder.Services.AddScoped<IAuthRepository, AuthRepository>();


            // Services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ITokenService, TokenService>();

            // Factories
            builder.Services.AddScoped<IAuthFactory, AuthFactory>();

            // Validators
            builder.Services.AddScoped<IEmailValidation, EmailValidation>();
            builder.Services.AddScoped<IPasswordValidation, PasswordValidation>();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer"
                });
            });

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
