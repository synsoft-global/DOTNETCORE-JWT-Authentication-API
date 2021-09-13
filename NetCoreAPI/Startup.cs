using NetCoreAPI.Middlewares;
using NetCoreBLL.Helper;
using NetCoreBLL.Interface;
using NetCoreBLL.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace NetCoreAPI
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
            /* It is used for cros origin start*/
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
            /* It is used for cros origin End*/

            /* It Is Used For Resource Based Model Validation Start */
            services.AddMvc().AddNewtonsoftJson().AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix).AddDataAnnotationsLocalization();
            /* It Is Used For Resource Based Model Validation End */

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "NetCoreAPI", Version = "v1" });
            });

            /* It Is Start Used For Dependency Injection */
            services.AddScoped<IJWTGenerator, JWTGenerator>();
            services.AddScoped<IAccount, AccountRepo>();
            services.AddScoped<IHome, HomeRepo>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "NetCoreAPI v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            // It is use to  Allow Cors Policy
            app.UseCors("AllowAll");

            /* It Is Enable Custome Mideelewares For Validate JWT */
            app.UseMiddleware<UseJWTValidator>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
