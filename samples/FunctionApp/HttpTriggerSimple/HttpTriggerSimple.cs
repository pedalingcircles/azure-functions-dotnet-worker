﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace FunctionApp
{
    public static class HttpTriggerSimple
    {
[Function(nameof(HttpTriggerSimple))]
public static HttpResponseData Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req,
    FunctionContext executionContext)
{
    var sw = new Stopwatch();
    sw.Restart();

    var logger = executionContext.GetLogger("FunctionApp.HttpTriggerSimple");
    logger.LogInformation("Message logged");

    // Manually deserialize the request body
    MyPoco? myPoco = null;
    using (var reader = new StreamReader(req.Body))
    {
        var body = reader.ReadToEnd();
        if (!string.IsNullOrWhiteSpace(body))
        {
            myPoco = JsonSerializer.Deserialize<MyPoco>(body);
        }
    }

    var response = req.CreateResponse(HttpStatusCode.OK);

    response.Headers.Add("Date", "Mon, 18 Jul 2016 16:06:00 GMT");
    response.Headers.Add("Content-Type", "text/html; charset=utf-8");
    response.WriteString("Hello world from HttpTriggerSimple!");

    logger.LogMetric(@"funcExecutionTimeMs", sw.Elapsed.TotalMilliseconds,
        new Dictionary<string, object> {
            { "foo", "bar" },
            { "baz", 42 }
        }
    );

    return response;
}
    }

    public class MyPoco
    {
        public string? Name { get; set; }
    }
}
