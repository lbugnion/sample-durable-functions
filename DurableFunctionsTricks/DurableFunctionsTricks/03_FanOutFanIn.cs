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
    public static class FanOutFanIn
    {
        [FunctionName(nameof(FanOutFanIn))]
        public static async Task<List<string>> FanOutFanInRunOrchestrator(
            [OrchestrationTrigger]
            IDurableOrchestrationContext context)
        {
            // Parallel calls

            var tasks = new List<Task<string>>();

            tasks.Add(context.CallActivityAsync<string>(
                nameof(FanOutFanInSayHello), 
                "Tokyo"));

            tasks.Add(context.CallActivityAsync<string>(
                nameof(FanOutFanInSayHello), 
                "Seattle"));

            tasks.Add(context.CallActivityAsync<string>(
                nameof(FanOutFanInSayHello), 
                "London"));

            var outputs = await Task.WhenAll(tasks);

            return outputs.ToList();
        }

        [FunctionName(nameof(FanOutFanInSayHello))]
        public static async Task<string> FanOutFanInSayHello(
            [ActivityTrigger]
            string name,
            ILogger log)
        {
            await Task.Delay(Random.Shared.Next(5000, 10000));
            log.LogDebug($"Saying hello to {name}.");
            return $"Hello {name}!";
        }

        [FunctionName(nameof(FanOutFanInHttpStart))]
        public static async Task<HttpResponseMessage> FanOutFanInHttpStart(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = "03")]
            HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            string instanceId = await starter.StartNewAsync(nameof(FanOutFanIn), null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}