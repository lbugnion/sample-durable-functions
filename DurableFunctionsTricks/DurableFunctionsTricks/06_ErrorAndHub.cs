using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace DurableFunctionsTricks
{
    /// <summary>
    /// Use this to explain the flow of the durable function orchestration.
    /// </summary>
    public static class ErrorAndHub
    {
        [FunctionName(nameof(ErrorAndHub))]
        public static async Task<List<string>> ErrorAndHubRunOrchestrator(
            [OrchestrationTrigger]
            IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            // Serial calls

            outputs.Add(await context.CallActivityAsync<string>(
                nameof(ErrorAndHubSayHello), "Tokyo"));

            outputs.Add(await context.CallActivityAsync<string>(
                nameof(ErrorAndHubSayHello), "Seattle"));

            outputs.Add(await context.CallActivityAsync<string>(
                nameof(ErrorAndHubSayHello), "London"));

            return outputs;
        }

        [FunctionName(nameof(ErrorAndHubSayHello))]
        public static string ErrorAndHubSayHello(
            [ActivityTrigger]
            string name,
            ILogger log)
        {
            log.LogDebug($"Saying hello to {name}.");

            if (name == "Seattle")
            {
                throw new System.Exception("Test error only");
            }

            return $"Hello {name}!";
        }

        [FunctionName(nameof(ErrorAndHubHttpStart))]
        public static async Task<HttpResponseMessage> ErrorAndHubHttpStart(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = "06")]
            HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            string instanceId = await starter.StartNewAsync(nameof(ErrorAndHub), null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}