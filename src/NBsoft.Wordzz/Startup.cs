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
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NBsoft.Logs;
using NBsoft.Wordzz.Infrastructure;
using NBsoft.Wordzz.Middleware;
using NBsoft.Wordzz.Models;
using NBsoft.Wordzz.Extensions;

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
            services.AddControllers();
            services.AddAuthentication(SessionKeyAuthOptions.AuthenticationScheme)
                .AddScheme<SessionKeyAuthOptions, SessionKeyAuthHandler>(SessionKeyAuthOptions.AuthenticationScheme, "", options => { });


            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Version = "v1", Title = "Wordzz" });
                options.OperationFilter<SessionKeyHeaderOperationFilter>();
            });

            var settings = Configuration.Get<AppSettings>();
            services.RegisterSettings(settings);

            SetupLoggers(services, settings);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI(x =>
            {
                x.SwaggerEndpoint("/swagger/v1/swagger.json", "NBsoft.Wordzz v1");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static void SetupLoggers(IServiceCollection services, AppSettings settings)
        {
            var loggerAggregate = new LoggerAggregate();
            loggerAggregate.AddLogger(new ConsoleLogger());
            loggerAggregate.AddLogger(new FileLogger(".\\Logs"));
            

            services.AddSingleton<Logs.Interfaces.ILogger>(loggerAggregate);
            Program.Log = loggerAggregate;

            Program.Log.WriteInfo("Backend", "Startup", null, "Logger Started");
        }
    }
}
