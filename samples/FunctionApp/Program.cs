using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry.Resources;

namespace FunctionApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = FunctionsApplication.CreateBuilder(args);

            builder.Services.AddOpenTelemetry()
                .UseAzureMonitorExporter();


            // ✅ Enable logging
            // builder.Logging.AddConsole(); // Optional, for local dev
            builder.Logging.ClearProviders(); // Remove default ILogger providers
            builder.Logging.AddOpenTelemetry(logging =>
            {
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
                logging.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MyApp"));
                logging.AddAzureMonitorLogExporter(options =>
                {
                    options.ConnectionString = "";
                });
            });

            var host = builder.Build();
            await host.RunAsync();
        }
    }
}
