using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore;
using Swashbuckle.AspNetCore.Swagger;
using System.IO;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.EntityFrameworkCore;
using Api.Database;
using PhilosophicalMonkey;
using Threenine.AutoMapperConfig;





namespace Api
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddDbContext<ApiContext>(options => options.UseSqlServer(Configuration.GetConnectionString("PortalDB")));
            services.AddMvc();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                new Info
                {
                    Title = "Stop Web Crawlers Update API",
                    Version = "v1",
                    Description = "Stop Web Crawlers Update API to enable the update of Referer Spammer Lists",
                    TermsOfService = "None",
                    Contact = new Contact { Name = "threenine.co.uk", Email = "support@threenine.co.uk", Url = "https://threenine.co.uk" }
                });
                var filePath = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "SWCapi.xml");
                c.IncludeXmlComments(filePath);
            }

            );


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            var options = new JwtBearerOptions
            {
                Audience = "https://swcapi.threenine.co.uk",
                Authority = "https://swcapi.auth0.com/"
            };
            app.UseJwtBearerAuthentication(options);

            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "SWC API V1");
            });

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                if (!serviceScope.ServiceProvider.GetService<ApiContext>().AllMigrationsApplied())
                {
                    serviceScope.ServiceProvider.GetService<ApiContext>().Database.Migrate();
                    serviceScope.ServiceProvider.GetService<ApiContext>().EnsureSeeded();
                }
            }

            //Set up code for automapper configuration 
            var seedTypes = new Type[] { typeof(Api.Domain.Marker) };
            var assemblies = Reflect.OnTypes.GetAssemblies(seedTypes);
            var typesInAssemblies = Reflect.OnTypes.GetAllExportedTypes(assemblies);
            MappingConfigurationFactory.LoadAllMappings(typesInAssemblies);

        }
    }
}
