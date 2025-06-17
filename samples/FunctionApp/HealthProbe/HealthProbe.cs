// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.Diagnostics.Metrics;

namespace FunctionApp
{
    public class HealthProbe(ILogger<HealthProbe> _logger, Instrumentation _instrumentation, Meter _meter)
    {

        [Function("HealthProbe")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            try
            {
                {

                    // Start of new trace, since this has not parent is it will create a new root trace.
                    using var activity = _instrumentation.ActivitySource.StartActivity("HealthProbeFunction.Root", ActivityKind.Server);
                    activity?.SetTag("http.method", req.Method);
                    activity?.SetTag("http.url", req.Url?.ToString());
                    activity?.SetTag("faas.trigger", "http");
                    activity?.SetTag("caller", "HealthProbe");

                    activity?.SetStatus(ActivityStatusCode.Ok, "Health probe is running");
                    _logger.LogInformation("Starting health probe");

                    // Simulate three asynchronous operations, each sleeping for 1 second
                    activity?.SetStatus(ActivityStatusCode.Ok, "Calling DummyAsyncOperation1");
                    await DummyAsyncOperation1();
                    await DummyAsyncOperation2();
                    await DummyAsyncOperation3();
                    _logger.LogInformation("Exiting health probe");
               
                    var counter = _meter.CreateCounter<long>("HealthProbeCounter", "1.0.0", "A counter for health probe invocations");
                    counter.Add(1, new KeyValuePair<string, object?>("source", "http"));

                    return new OkObjectResult("Healthy!");
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking health status.");
                return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
            }
        }

        private async Task DummyAsyncOperation1()
        {
            using var activity = _instrumentation.ActivitySource.StartActivity("DummyAsyncOperation1");
            activity?.SetTag("operation", "DummyAsyncOperation1");

            await Task.Delay(500); // Simulate async work by sleeping for 1 second
        }


        private async Task DummyAsyncOperation2()
        {
            using var activity = _instrumentation.ActivitySource.StartActivity("DummyAsyncOperation2");
            activity?.SetTag("operation", "DummyAsyncOperation2");
            await Task.Delay(600); // Simulate async work by sleeping for 1 second
        }

        private async Task DummyAsyncOperation3()
        {
            using var activity = _instrumentation.ActivitySource.StartActivity("DummyAsyncOperation2");
            activity?.SetTag("operation", "DummyAsyncOperation2");
            await Task.Delay(400); // Simulate async work by sleeping for 1 second
        }
    }
}
