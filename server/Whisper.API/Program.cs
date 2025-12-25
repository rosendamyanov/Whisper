using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using Whisper.Authentication.Configuration;
using Whisper.Authentication.Data;
using Whisper.Authentication.Data.Interfaces;
using Whisper.Authentication.Factory;
using Whisper.Authentication.Factory.Interfaces;
using Whisper.Authentication.Services;
using Whisper.Authentication.Services.Interfaces;
using Whisper.Services.Validation;
using Whisper.Services.Validation.Interfaces;
using Whisper.Data.Context;
using Whisper.Data.Repositories;
using Whisper.Data.Repositories.Interfaces;
using Whisper.Services;
using Whisper.Services.Factories;
using Whisper.Services.Factories.ChatFactory;
using Whisper.Services.Factories.Interfaces;
using Whisper.Services.Hubs;
using Whisper.Services.Services;
using Whisper.Services.Services.Interfaces;

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
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                    ValidAudience = builder.Configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]!))
                };

                // Read token from cookie
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // First, check for SignalR query string token
                        var path = context.HttpContext.Request.Path;
                        if (path.StartsWithSegments("/hubs"))
                        {
                            var accessToken = context.Request.Query["access_token"];
                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                context.Token = accessToken;
                                return Task.CompletedTask;
                            }
                        }

                        // Fall back to cookie for regular HTTP requests
                        context.Token = context.Request.Cookies["AccessToken"];
                        return Task.CompletedTask;
                    }
                };
            });

            // Add services to the container.
            // Repositories
            builder.Services.AddScoped<IAuthRepository, AuthRepository>();
            builder.Services.AddScoped<IChatRepository, ChatRepository>();
            builder.Services.AddScoped<IFriendshipRepository, FriendshipRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IMessageRepository, MessageRepository>();


            // Services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IChatService, ChatService>();
            builder.Services.AddScoped<IFriendshipService, FriendshipService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IMessageService, MessageService>();
            builder.Services.AddScoped<ILocalFileStorageService, LocalFileStorageService>();
            builder.Services.AddScoped<IChatNotificationService, ChatNotificationService>();
            builder.Services.AddSingleton<IPresenceTracker, PresenceTracker>();

            // Factories
            builder.Services.AddScoped<IAuthFactory, AuthFactory>();
            builder.Services.AddScoped<IChatFactory, ChatFactory>();
            builder.Services.AddScoped<IFriendshipFactory, FriendshipFactory>();
            builder.Services.AddScoped<IUserFactory, UserFactory>();
            builder.Services.AddScoped<IMessageFactory, MessageFactory>();

            // Validators
            builder.Services.AddScoped<IEmailValidation, EmailValidation>();
            builder.Services.AddScoped<IPasswordValidation, PasswordValidation>();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSignalR();
            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer"
                });

                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Whisper API",
                    Version = "v1",
                    Description = "Real-time communication API for Whisper application"
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:5173", 
                                           "https://localhost:5173",
                                           "null") 
                                                                     
                              .AllowCredentials()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors(MyAllowSpecificOrigins);
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapHub<ChatHub>("/hubs/chat");
            app.MapHub<VoiceHub>("/hubs/voice");
            app.MapHub<StreamHub>("/hubs/stream");


            app.MapControllers();

            app.Run();
        }
    }
}
