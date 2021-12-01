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
    public static class FunctionChaining
    {
        [FunctionName(nameof(FunctionChaining))]
        public static async Task<List<string>> FunctionChainingRunOrchestrator(
            [OrchestrationTrigger] 
            IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            // Serial calls

            outputs.Add(await context.CallActivityAsync<string>(
                nameof(FunctionChainingSayHello), "Tokyo"));
            
            outputs.Add(await context.CallActivityAsync<string>(
                nameof(FunctionChainingSayHello), "Seattle"));

            outputs.Add(await context.CallActivityAsync<string>(
                nameof(FunctionChainingSayHello), "London"));

            return outputs;
        }

        [FunctionName(nameof(FunctionChainingSayHello))]
        public static string FunctionChainingSayHello(
            [ActivityTrigger] 
            string name, 
            ILogger log)
        {
            log.LogDebug($"Saying hello to {name}.");
            return $"Hello {name}!";
        }

        [FunctionName(nameof(FunctionChainingHttpStart))]
        public static async Task<HttpResponseMessage> FunctionChainingHttpStart(
            [HttpTrigger(
                AuthorizationLevel.Anonymous, 
                "get",
                Route = "01")]
            HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            string instanceId = await starter.StartNewAsync(nameof(FunctionChaining), null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}