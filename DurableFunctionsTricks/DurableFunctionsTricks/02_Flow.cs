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
    /// Use this to demonstrate the usage of the statusQueryGetUri in the Start response.
    /// </summary>
    public static class Flow
    {
        [FunctionName(nameof(Flow))]
        public static async Task<List<string>> FlowRunOrchestrator(
            [OrchestrationTrigger]
            IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            // Serial calls

            outputs.Add(await context.CallActivityAsync<string>(
                nameof(FlowSayHello), "Tokyo"));

            outputs.Add(await context.CallActivityAsync<string>(
                nameof(FlowSayHello), "Seattle"));

            outputs.Add(await context.CallActivityAsync<string>(nameof(
                FlowSayHello), "London"));

            return outputs;
        }

        [FunctionName(nameof(FlowSayHello))]
        public static async Task<string> FlowSayHello(
            [ActivityTrigger]
            string name,
            ILogger log)
        {
            await Task.Delay(10000);
            log.LogDebug($"Saying hello to {name}.");
            return $"Hello {name}!";
        }

        [FunctionName(nameof(FlowHttpStart))]
        public static async Task<HttpResponseMessage> FlowHttpStart(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = "02")]
            HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            string instanceId = await starter.StartNewAsync(nameof(Flow), null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}