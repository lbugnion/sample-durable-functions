using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace DurableFunctionsTricks
{
    public static class AwaitInOrchestrator
    {
        [FunctionName(nameof(AwaitInOrchestrator))]
        public static async Task<List<string>> AwaitInOrchestratorRunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            var client = new HttpClient();

            var randomName = await client.GetStringAsync(
                "http://localhost:7071/api/get-random-name");

            outputs.Add(await context.CallActivityAsync<string>(
                nameof(AwaitInOrchestratorSayHello), 
                randomName));

            return outputs;
        }

        [FunctionName(nameof(AwaitInOrchestratorSayHello))]
        public static string AwaitInOrchestratorSayHello(
            [ActivityTrigger] 
            string name, 
            ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }

        [FunctionName(nameof(AwaitInOrchestratorHttpStart))]
        public static async Task<HttpResponseMessage> AwaitInOrchestratorHttpStart(
            [HttpTrigger(
                AuthorizationLevel.Anonymous, 
                "get",
                Route = "09")] 
            HttpRequestMessage req,
            [DurableClient] 
            IDurableOrchestrationClient starter,
            ILogger log)
        {
            string instanceId = await starter.StartNewAsync(nameof(AwaitInOrchestrator), null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}