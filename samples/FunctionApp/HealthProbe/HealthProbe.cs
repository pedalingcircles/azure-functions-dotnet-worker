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

namespace FunctionApp
{
    public class HealthProbe(ILogger<HealthProbe> _logger)
    {
        [Function("HealthProbe")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            try
            {
                // Simulate three asynchronous operations, each sleeping for 1 second
                await DummyAsyncOperation1();
                await DummyAsyncOperation2();
                await DummyAsyncOperation3();
                return new OkObjectResult("Healthy!");
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
