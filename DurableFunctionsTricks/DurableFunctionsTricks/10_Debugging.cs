using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace DurableFunctionsTricks
{
    /// <summary>
    /// Use this to demo the fan out - fan in pattern.
    /// </summary>
    public static class Debugging
    {
        [FunctionName(nameof(Debugging))]
        public static async Task<List<string>> DebuggingRunOrchestrator(
            [OrchestrationTrigger]
            IDurableOrchestrationContext context)
        {
            // Parallel calls

            var tasks = new List<Task<string>>();

            for (var index = 0; index < 10; index++)
            {
                tasks.Add(context.CallActivityAsync<string>(
                    nameof(DebuggingSayHello), 
                    null));
            }

            var outputs = await Task.WhenAll(tasks);
            return outputs.ToList();
        }

        [FunctionName(nameof(DebuggingSayHello))]
        public static async Task<string> DebuggingSayHello(
            [ActivityTrigger]
            string name,
            ILogger log)
        {
            var client = new HttpClient();
            var randomName = await client.GetStringAsync("http://localhost:7071/api/get-random-name");
            return randomName;
        }

        [FunctionName(nameof(DebuggingHttpStart))]
        public static async Task<HttpResponseMessage> DebuggingHttpStart(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = "10")]
            HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            string instanceId = await starter.StartNewAsync(nameof(Debugging), null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}