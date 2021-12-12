using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Dx29.Services;

namespace Dx29.Exomiser.Worker
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();

            services.AddSingleton((c) => new BlobStorage(Configuration.GetConnectionString("ExomiserBlobStorage")));
            services.AddSingleton((c) => new ServiceBus(Configuration.GetConnectionString("ExomiserServiceBus"), Configuration["ExomiserQueueName"]));

            services.AddSingleton<ExomiserService>();
            services.AddSingleton<ExomiserDispatcher>();
        }
    }
}
