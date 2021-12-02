using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace DurableFunctionsTricks
{
    public static class Deterministic
    {
        [FunctionName(nameof(Deterministic))]
        public static async Task<List<string>> DeterministicRunOrchestrator(
            [OrchestrationTrigger]
            IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            var info = context.GetInput<TriggerInfo>();

            // Serial calls

            var output = await context.CallActivityAsync<string>(
                nameof(DeterministicSayHello), info.MyVariable);

            outputs.Add(output);

            // TODO Save to online storage / database

            output = await context.CallActivityAsync<string>(
                nameof(DeterministicSayHello), "Tokyo");

            outputs.Add(output);

            // TODO Save to online storage / database

            output = await context.CallActivityAsync<string>(
                nameof(DeterministicSayHello), "London");

            outputs.Add(output);

            // TODO Save to online storage / database

            return outputs;
        }

        [FunctionName(nameof(DeterministicSayHello))]
        public static string DeterministicSayHello(
            [ActivityTrigger]
            string name,
            ILogger log)
        {
            log.LogDebug($"Saying hello to {name}.");
            return $"Hello {name}!";
        }

        [FunctionName(nameof(DeterministicHttpStart))]
        public static async Task<HttpResponseMessage> DeterministicHttpStart(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = "05")]
            HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var info = new TriggerInfo
            {
                MyVariable = Environment.GetEnvironmentVariable("USERNAME")
            };

            string instanceId = await starter.StartNewAsync(nameof(Deterministic), info);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        public class TriggerInfo
        {
            public string MyVariable { get; set; }
        }
    }
}