using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Azure.Monitor.OpenTelemetry.Exporter;

namespace FunctionApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = FunctionsApplication.CreateBuilder(args);

            builder.Services.AddOpenTelemetry()
                .UseAzureMonitorExporter();

            var host = builder.Build();
            await host.RunAsync();
        }
    }
}
