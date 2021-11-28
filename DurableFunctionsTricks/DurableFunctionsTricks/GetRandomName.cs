using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace DurableFunctionsTricks
{
    public static class GetRandomName
    {
        [FunctionName(nameof(GetRandomName))]
        public static string Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous, 
                "get",
                Route = "get-random-name")] 
            HttpRequest req,
            ILogger log)
        {
            var names = new List<string>
            {
                "Zurich",
                "Bern",
                "Lausanne",
                "Oslo"
            };

            return names[Random.Shared.Next(0, names.Count - 1)];
        }
    }
}
