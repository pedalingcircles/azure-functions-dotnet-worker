using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace FunctionApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = FunctionsApplication.CreateBuilder(args);

            builder.Services.AddSingleton<Instrumentation>();

            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(serviceName: "MyApp", serviceVersion: "3.2.1", serviceNamespace: "Samples.MyFunctionApp")
                .AddAttributes(new[]
                {
                    new KeyValuePair<string, object>("deployment.environment", "local"),
                    new KeyValuePair<string, object>("region", "eastus2")
                });

            builder.Services.AddOpenTelemetry()
                .WithTracing(tracing =>
                {
                    tracing
                        .SetResourceBuilder(resourceBuilder)
                        .AddSource("functionapp-server") // Add your ActivitySource name here
                        .AddAzureMonitorTraceExporter(options =>
                        {
                            options.ConnectionString = "";
                        });
                });
                // .UseAzureMonitorExporter();

            // ✅ Enable logging
            // builder.Logging.AddConsole(); // Optional, for local dev
            builder.Logging.ClearProviders(); // Remove default ILogger providers
            builder.Logging.AddOpenTelemetry(logging =>
            {
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
                logging.SetResourceBuilder(resourceBuilder);
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
