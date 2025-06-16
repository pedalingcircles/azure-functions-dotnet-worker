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

namespace FunctionApp
{
    public class HealthProbe(ILogger<HealthProbe> _logger)
    {

        [Function("HealthProbe")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            try
            {
                using (_logger.BeginScope(new Dictionary<string, object?>
                {
                    ["HttpMethod"] = req.Method,
                    ["Path"] = req.Url?.AbsolutePath,
                    ["Host"] = req.Url?.Host
                }))
                {
                    _logger.LogInformation("Starting health probe");

                    // Added to capture requests
                    var activitySource = new ActivitySource("MyApp.HealthProbe");
                    using (var activity = activitySource.StartActivity("MyApp.HealthProbe", ActivityKind.Server))
                    {
                        activity?.SetTag("http.method", req.Method);
                        activity?.SetTag("http.url", req.Url?.ToString());
                        activity?.SetTag("faas.trigger", "http");

                    }



                    // Simulate three asynchronous operations, each sleeping for 1 second
                    await DummyAsyncOperation1();
                    await DummyAsyncOperation2();
                    await DummyAsyncOperation3();
                    _logger.LogInformation("Exiting health probe");
                    return new OkObjectResult("Healthy!");
                    
                }


            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking health status.");
                return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
            }
        }

        private static async Task DummyAsyncOperation1()
        {
            await Task.Delay(500); // Simulate async work by sleeping for 1 second
        }


        private static async Task DummyAsyncOperation2()
        {
            await Task.Delay(600); // Simulate async work by sleeping for 1 second
        }

        private static async Task DummyAsyncOperation3()
        {
            await Task.Delay(400); // Simulate async work by sleeping for 1 second
        }
    }
}
