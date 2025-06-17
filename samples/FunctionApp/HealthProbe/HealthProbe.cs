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
using Azure;

namespace FunctionApp
{
    public class HealthProbe(ILogger<HealthProbe> _logger, Instrumentation _instrumentation)
    {

        [Function("HealthProbe")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object?>
            {
                ["Foo"] = "bar",
                ["Bang"] = "buzz"
            });

            // Start of new trace, since this has not parent is it will create a new root trace.
            // In order for this to be a special Request trace 
            //      ActivityKind must be set to Server.
            //      http.method, http.url tags must be set
            // Optional and recommended tags are:
            //      faas.trigger especially for non HTTP triggers see: https://opentelemetry.io/docs/specs/semconv/faas/faas-spans/ and https://opentelemetry.io/docs/specs/semconv/registry/attributes/faas/
            using var activity = _instrumentation.ActivitySource.StartActivity("HandleHealthProbeRequest", ActivityKind.Server);
            activity?.SetTag("http.method", req.Method);
            activity?.SetTag("http.url", req.Url?.ToString());
            activity?.SetTag("faas.trigger", "http");
            activity?.SetTag("caller", "HealthProbe");
            HttpResponseData? response = null;

            try
            {
                // activity?.SetStatus(ActivityStatusCode.Ok, "Health probe is running");
                _logger.LogInformation("Starting health probe");

                // Simulate three asynchronous operations, each sleeping for 1 second
                // activity?.SetStatus(ActivityStatusCode.Ok, "Calling DummyAsyncOperation1");
                await DummyAsyncOperation1();
                await DummyAsyncOperation2();
                await DummyAsyncOperation3();
                _logger.LogInformation("Exiting health probe");

                response = req.CreateResponse(System.Net.HttpStatusCode.OK);
                response.WriteString("Service Available");
                return response;
            }
            catch (System.Exception ex)
            {
                //_logger.LogError(ex, "An error occurred while checking health status.");
                activity?.SetStatus(ActivityStatusCode.Error);
                activity?.SetTag("otel.status_code", "ERROR");
                activity?.SetTag("exception.message", ex.Message);
                activity?.SetTag("exception.stacktrace", ex.StackTrace);

                response = req.CreateResponse(System.Net.HttpStatusCode.ServiceUnavailable);
                response.WriteString("Service Unavailable");
                return response;
            }
            finally
            {
                if (response != null)
                {
                    activity?.SetTag("http.status_code", (int)response.StatusCode);
                }
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
