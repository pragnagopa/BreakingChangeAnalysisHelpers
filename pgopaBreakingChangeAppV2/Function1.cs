using System;
using System.IO;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Data.OData;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authentication.Google;

namespace pgopaBreakingChangeAppV2
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            log.LogInformation($"{GoogleDefaults.AuthenticationScheme}");
            IDataReader reader;
            JObject x = new JObject();
            //ODataMessageWriterSettings settings = new ODataMessageWriterSettings();
            SqlConnection connection = new SqlConnection("");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";
            //throw new Exception("test ex");
            log.LogError(new Exception("testex"), "failed");
            return new OkObjectResult(responseMessage);
        }
    }
}
