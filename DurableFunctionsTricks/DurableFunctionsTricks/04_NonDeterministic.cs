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
    /// <summary>
    /// 
    /// </summary>
    public static class NonDeterministic
    {
        [FunctionName(nameof(NonDeterministic))]
        public static async Task<List<string>> NonDeterministicRunOrchestrator(
            [OrchestrationTrigger]
            IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            // Serial calls

            var variable = Environment.GetEnvironmentVariable("MyVariable");

            var output = await context.CallActivityAsync<string>(
                nameof(NonDeterministicSayHello), variable);

            outputs.Add(output);
            File.AppendAllText("c:\\temp\\output.txt", output);

            output = await context.CallActivityAsync<string>(
                nameof(NonDeterministicSayHello), "Tokyo");

            outputs.Add(output);
            File.AppendAllText("c:\\temp\\output.txt", output);

            output = await context.CallActivityAsync<string>(
                nameof(NonDeterministicSayHello), "London");

            outputs.Add(output);
            File.AppendAllText("c:\\temp\\output.txt", output);

            return outputs;
        }

        [FunctionName(nameof(NonDeterministicSayHello))]
        public static string NonDeterministicSayHello(
            [ActivityTrigger]
            string name,
            ILogger log)
        {
            log.LogDebug($"Saying hello to {name}.");
            return $"Hello {name}!";
        }

        [FunctionName(nameof(NonDeterministicHttpStart))]
        public static async Task<HttpResponseMessage> NonDeterministicHttpStart(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = "04")]
            HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync(nameof(NonDeterministic), null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}