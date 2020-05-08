using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using NBsoft.Logs;
using NBsoft.Wordzz.Infrastructure;
using NBsoft.Wordzz.Models;
using NBsoft.Wordzz.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using NBsoft.Wordzz.Services;
using NBsoft.Wordzz.Hubs;
using NBsoft.Logs.Sql;

namespace NBsoft.Wordzz
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
            {
                builder
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .WithOrigins("http://localhost:4200", "http://wordzz.nbsoft.pt");
                
            }));
            services.AddControllers();

            var appSettingsSection = Configuration.GetSection("Wordzz");
            services.Configure<WordzzSettings>(appSettingsSection);
            
            var appSettings = appSettingsSection.Get<WordzzSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.EncryptionKey);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;                
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
                x.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments(GameHub.Address)))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddSignalR();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Version = "v1", Title = "Wordzz" });
                options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Scheme = "bearer"
                });
                options.OperationFilter<JWTAuthOperationFilter>();
            });

            var settings = Configuration.Get<AppSettings>();

            SetupLoggers(services, settings);

            services.RegisterRepositories(settings)
                .RegisterServices();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            //app.UseCors(x => x
            //    .AllowAnyOrigin()
            //    .AllowAnyMethod()
            //    .AllowAnyHeader());
            app.UseCors("CorsPolicy");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI(x =>
            {
                x.SwaggerEndpoint("/swagger/v1/swagger.json", "NBsoft.Wordzz v1");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<GameHub>(GameHub.Address);
                endpoints.MapControllers();
            });
        }

        private static void SetupLoggers(IServiceCollection services, AppSettings settings)
        {
            var loggerAggregate = new LoggerAggregate();
            loggerAggregate.AddLogger(new ConsoleLogger());
            try
            {
                loggerAggregate.AddLogger(new MySqlLogger(settings.Wordzz.Db.LogConnString, "wordzzlogs"));
            }
            catch (Exception ex)
            {
                throw new Exception($"ConnString:{settings.Wordzz.Db.LogConnString}", ex);
            }
            

            services.AddSingleton<Logs.Interfaces.ILogger>(loggerAggregate);
            Program.Log = loggerAggregate;

            Program.Log.WriteInfo("Backend", "Startup", null, "Logger Started");
        }
    }
}
