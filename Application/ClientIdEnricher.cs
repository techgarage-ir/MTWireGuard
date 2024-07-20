using Serilog.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTWireGuard.Application
{
    public class ClientIdEnricher(string clientId) : ILogEventEnricher
    {
        private readonly string _clientId = clientId;

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ClientId", _clientId));
        }
    }

    public static class LoggingExtensions
    {
        public static LoggerConfiguration WithClientId(this LoggerEnrichmentConfiguration enrichmentConfiguration, string clientId)
        {
            return enrichmentConfiguration.With(new ClientIdEnricher(clientId));
        }
    }
}
