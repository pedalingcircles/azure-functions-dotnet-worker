// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Net;
using System.Diagnostics;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace FunctionApp
{
    public class HttpTriggerWithCancellation
    {
        private readonly ILogger _logger;
        private static readonly ActivitySource ActivitySource = new("FunctionApp");

        public HttpTriggerWithCancellation(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HttpTriggerWithCancellation>();
        }

        [Function(nameof(HttpTriggerWithCancellation))]
        public HttpResponseData Run(
            [HttpTrigger(AuthorizationLevel.Anonymous,"get", "post", Route = null)]
            HttpRequestData req,
            FunctionContext executionContext,
            CancellationToken cancellationToken)
        {

    // Start a logging scope with some test properties
    using (_logger.BeginScope(new Dictionary<string, object>
    {
        ["ScopeKey"] = "ScopeValue",
        ["RequestId"] = Guid.NewGuid().ToString()
    }))
    {
        // Start an Activity with ActivityKind.Server
        using var activity = ActivitySource.StartActivity(
            "HttpTriggerWithCancellation",
            ActivityKind.Server);

        // Set standard HTTP attributes
        activity?.SetTag("http.method", req.Method);
        activity?.SetTag("http.url", req.Url.ToString());

        _logger.LogInformation("Hello from inside a logging scope!");

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.WriteString($"Hello world!");
            return response;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("A cancellation token was received. Taking precautionary actions.");

            var response = req.CreateResponse(HttpStatusCode.ServiceUnavailable);
            response.WriteString("Invocation cancelled");
            return response;
        }
    }
        }
    }
}
