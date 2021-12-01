using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace DurableFunctionsTricks
{
    public static class AwaitInOrchestratorProperly
    {
        [FunctionName(nameof(AwaitInOrchestratorProperly))]
        public static async Task<List<string>> AwaitInOrchestratorProperlyRunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            var randomName = await context.CallActivityAsync<string>(
                nameof(CallGetRandomName), 
                null);

            outputs.Add(await context.CallActivityAsync<string>(
                nameof(AwaitInOrchestratorProperlySayHello),
                randomName));

            return outputs;
        }

        [FunctionName(nameof(CallGetRandomName))]
        public static async Task<string> CallGetRandomName(
            [ActivityTrigger]
            ILogger log)
        {
            var client = new HttpClient();
            var randomName = await client.GetStringAsync(
                "http://localhost:7071/api/get-random-name");
            return randomName;
        }

        [FunctionName(nameof(AwaitInOrchestratorProperlySayHello))]
        public static string AwaitInOrchestratorProperlySayHello(
            [ActivityTrigger]
            string name,
            ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }

        [FunctionName(nameof(AwaitInOrchestratorProperlyHttpStart))]
        public static async Task<HttpResponseMessage> AwaitInOrchestratorProperlyHttpStart(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = "09A")]
            HttpRequestMessage req,
            [DurableClient]
            IDurableOrchestrationClient starter,
            ILogger log)
        {
            string instanceId = await starter.StartNewAsync(nameof(AwaitInOrchestratorProperly), null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}