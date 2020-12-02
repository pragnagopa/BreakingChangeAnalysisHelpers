using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Reflection;

namespace PgopaPinToV2Helper
{
    public class Function1
    {
        ILogger _systemLogger;
        public Function1(ILoggerFactory loggerFactory)
        {
            _systemLogger = loggerFactory.CreateLogger("Host.General.BreakingApp");
        }


        [FunctionName("Function1")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            _systemLogger.LogInformation("System log");
            string name = req.Query["name"];
            var binDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var rootDirectory = Path.GetFullPath(Path.Combine(binDirectory, ".."));

            //string[] breakingApps = File.ReadAllLines(Path.Join(rootDirectory, "PublicCloudBreakingAppNamesInitialList.txt"));
            //foreach(var breakingApp in breakingApps)
            //{
            //    _systemLogger.LogInformation(breakingApp);
            //}

            string[] appsToLog = File.ReadAllLines(Path.Join(rootDirectory, "PublicWindowsBreakingAppsUD4.csv"));
            foreach (var appAndSubId in appsToLog)
            {
                string[] parsedAppsAndSubId = appAndSubId.Split(",");
                _systemLogger.LogInformation($"SubId={parsedAppsAndSubId[0]},AppId={parsedAppsAndSubId[1]}---end---");
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}
