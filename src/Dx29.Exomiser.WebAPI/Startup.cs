using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using Dx29.Services;

namespace Dx29.Exomiser.WebAPI
{
    public class Startup
    {
        public const string VERSION = "v0.0.1";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Dx29.Exomiser.WebAPI", Version = "v1" });
            });

            services.AddSingleton((c) => new BlobStorage(Configuration.GetConnectionString("ExomiserBlobStorage")));
            services.AddSingleton((c) => new ServiceBus(Configuration.GetConnectionString("ExomiserServiceBus"), Configuration["ExomiserQueueName"]));

            services.AddSingleton<ExomiserClient>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dx29.Exomiser.WebAPI v1"));

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
