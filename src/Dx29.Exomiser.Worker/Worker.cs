using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

namespace Dx29.Exomiser.Worker
{
    public class Worker : BackgroundService
    {
#if  DEBUG
        const int DELAY_MINUTES = 1;
#else
        const int DELAY_MINUTES = 20;
#endif

        public Worker(ExomiserDispatcher dispatcher, IHostApplicationLifetime applicationLifetime)
        {
            Dispatcher = dispatcher;
            ApplicationLifetime = applicationLifetime;
        }

        public ExomiserDispatcher Dispatcher { get; }
        public IHostApplicationLifetime ApplicationLifetime { get; }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("{0}\tExecuting...", DateTimeOffset.UtcNow);

            Dispatcher.Options.MaxConcurrentCalls = 1;
            Dispatcher.Options.MaxAutoRenewDuration = TimeSpan.FromMinutes(10); // Maximun 20 mins to finish job execution
            await Dispatcher.RunAsync(minutes: DELAY_MINUTES, cancellationToken); // Wait 10 mins to exit since last handled message
            ApplicationLifetime.StopApplication();

            Console.WriteLine("{0}\tDone!", DateTimeOffset.UtcNow);
        }
    }
}
