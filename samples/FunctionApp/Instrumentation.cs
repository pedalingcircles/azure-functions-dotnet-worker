using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace FunctionApp;

public sealed class Instrumentation : IDisposable
{
    internal const string ActivitySourceName = "FunctionApp.Instrumentation";
    internal const string MeterName = "FunctionApp.Metrics";
    private readonly Meter meter;



    public Instrumentation()
    {
        string? version = typeof(Instrumentation).Assembly.GetName().Version?.ToString();
        this.ActivitySource = new ActivitySource(ActivitySourceName, version);
        this.meter = new Meter(MeterName, version);

        // custom metrics
        this.SuccessFileScanCounter = this.meter.CreateCounter<long>("filescan.requests.success", description: "The number of files that are successfully scanned.");
    }

    public ActivitySource ActivitySource { get; }

    public Counter<long> SuccessFileScanCounter { get; }

    public void Dispose()
    {
        this.ActivitySource.Dispose();
        this.meter.Dispose();
    }
}
