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
    public class EventGridHandlerDummy(ILogger<HealthProbe> _logger, Instrumentation _instrumentation)
    {

        [Function("EventGridHandlerDummy")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            
            

            using var scope = _logger.BeginScope(new Dictionary<string, object?>
            {
                ["fanchisecodename"] = "Antero",
                ["beta-name"] = "Azul"
            });


            // Read the request body as a string
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            // Deserialize the request body into a dynamic object (or replace with a specific type)
            var eventData = JsonSerializer.Deserialize<EventData>(requestBody);

            var id = eventData.id;
            var correlationId = eventData.data.correlationId;

            // Start of new trace, since this has not parent is it will create a new root trace.
            // In order for this to be a special Request trace 
            //      ActivityKind must be set to Server.
            //      http.method, http.url tags must be set
            // Optional and recommended tags are:
            //      faas.trigger especially for non HTTP triggers see: https://opentelemetry.io/docs/specs/semconv/faas/faas-spans/ and https://opentelemetry.io/docs/specs/semconv/registry/attributes/faas/
            using var activity = _instrumentation.ActivitySource.StartActivity("HandleDefenderEvent", ActivityKind.Consumer);
            activity?.SetTag("http.method", req.Method);
            activity?.SetTag("http.url", req.Url?.ToString());
            activity?.SetTag("faas.trigger", "pubsub");
            activity?.SetTag("caller", "Defender");

            // Setting event specific attributes
            activity?.SetTag("event.id", eventData.id); // could be correlation id
            activity?.SetTag("event.type", eventData.eventType);
            activity?.SetTag("event.subject", eventData.subject);
            activity?.SetTag("storage.resource", eventData.topic);
            activity?.SetTag("alert.type", eventData.eventType);

            HttpResponseData? response = null;

            try
            {
                // activity?.SetStatus(ActivityStatusCode.Ok, "Health probe is running");
                _logger.LogInformation("Start processing file");

                // Simulate three asynchronous operations, each sleeping for 1 second
                await ParseMessage();
                await ProcessScanResult();
                await ExtractFileMetaFromUri();
                _logger.LogInformation("Completed processing file");

                // Count the scanned file
                // _instrumentation.SuccessFileScanCounter.Add(1);
                _instrumentation.SuccessFileScanCounter.Add(1, KeyValuePair.Create<string, object?>("stage", "topic"));

                response = req.CreateResponse(System.Net.HttpStatusCode.OK);
                response.WriteString("File processed successfully");
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

        private async Task ParseMessage()
        {
            using var activity = _instrumentation.ActivitySource.StartActivity("ParseMessage");
            _logger.LogInformation("Parsing message");
            activity?.SetTag("storeid", "123");
            await Task.Delay(200); // Simulate async work by sleeping for 1 second
        }

        private async Task ProcessScanResult()
        {
            using var activity = _instrumentation.ActivitySource.StartActivity("ProcessScanResult");
            _logger.LogInformation("Processing scan result");
            activity?.SetTag("location", "TX");
            await Task.Delay(100); // Simulate async work by sleeping for 1 second
        }

        private async Task ExtractFileMetaFromUri()
        {
            using var activity = _instrumentation.ActivitySource.StartActivity("ExtractFileMetaFromUri");
            _logger.LogInformation("Extracting file metadata from URI");
            activity?.SetTag("parkinglot", "false");
            await Task.Delay(300); // Simulate async work by sleeping for 1 second
        }
    }

    public class EventData
    {
        public string id { get; set; }
        public string subject { get; set; }
        public Data data { get; set; }
        public string eventType { get; set; }
        public string dataVersion { get; set; }
        public string metadataVersion { get; set; }
        public DateTime eventTime { get; set; }
        public string topic { get; set; }
    }

    public class Data
    {
        public string correlationId { get; set; }
        public string blobUri { get; set; }
        public string eTag { get; set; }
        public DateTime scanFinishedTimeUtc { get; set; }
        public string scanResultType { get; set; }
        public string scanResultDetails { get; set; }
    }



}
