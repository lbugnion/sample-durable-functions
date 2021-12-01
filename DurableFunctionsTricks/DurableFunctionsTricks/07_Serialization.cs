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
    /// Use this to show the issue with internal properties due to serialization.
    /// </summary>
    public static class Serialization
    {
        [FunctionName(nameof(Serialization))]
        public static async Task<List<string>> SerializationRunOrchestrator(
            [OrchestrationTrigger]
            IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            // Serial calls

            var info = new Info
            {
                Name = "Tokyo"
            };

            outputs.Add(await context.CallActivityAsync<string>(
                nameof(SerializationSayHello), 
                info));

            return outputs;
        }

        [FunctionName(nameof(SerializationSayHello))]
        public static string SerializationSayHello(
            [ActivityTrigger]
            Info info,
            ILogger log)
        {
            log.LogDebug($"Saying hello to {info.Name}."); 
            return $"HELLO {info.Name.ToUpper()}!";
        }

        [FunctionName(nameof(SerializationHttpStart))]
        public static async Task<HttpResponseMessage> SerializationHttpStart(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = "07")]
            HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            string instanceId = await starter.StartNewAsync(nameof(Serialization), null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }

    public class Info
    {
        public string Name { get; internal set; }
    }
}