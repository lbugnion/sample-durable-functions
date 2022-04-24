using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DurableFunctionsTricks.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace DurableFunctionsTricks
{
    /// <summary>
    /// Use this to show the issue with internal properties due to SerializationRecursion.
    /// </summary>
    public static class SerializationRecursion
    {
        [FunctionName(nameof(SerializationRecursion))]
        public static async Task<List<string>> SerializationRecursionRunOrchestrator(
            [OrchestrationTrigger]
            IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            // Serial calls

            var series = new LearnLiveSeries();

            series.Modules = new List<LearnLiveModule>
            {
                new LearnLiveModule("First module")
                {
                    Parent = series
                }
            };

            outputs.Add(await context.CallActivityAsync<string>(
                nameof(SerializationRecursionSayHello), 
                series));

            return outputs;
        }

        [FunctionName(nameof(SerializationRecursionSayHello))]
        public static string SerializationRecursionSayHello(
            [ActivityTrigger]
            LearnLiveSeries series,
            ILogger log)
        {
            // DO WORK

            var result = $"Found {series.Modules.Count} modules";
            log.LogInformation(result);
            return result;
        }

        [FunctionName(nameof(SerializationRecursionHttpStart))]
        public static async Task<HttpResponseMessage> SerializationRecursionHttpStart(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = "08")]
            HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            string instanceId = await starter.StartNewAsync(nameof(SerializationRecursion), null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}