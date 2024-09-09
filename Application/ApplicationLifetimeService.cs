using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTWireGuard.Application
{
    public class ApplicationLifetimeService(IHostApplicationLifetime hostApplicationLifetime, ILogger logger) : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            hostApplicationLifetime.ApplicationStarted.Register(OnStarted);
            hostApplicationLifetime.ApplicationStopping.Register(OnStopping);
            hostApplicationLifetime.ApplicationStopped.Register(OnStopped);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        private void OnStarted()
        {
            logger.LogInformation("Registered: {Name}!", Environment.UserName);
        }

        private void OnStopping()
        {
            logger.LogWarning("Exiting: {MachineName}!", Environment.MachineName);
        }

        private void OnStopped()
        {
            // ...
        }
    }
}
