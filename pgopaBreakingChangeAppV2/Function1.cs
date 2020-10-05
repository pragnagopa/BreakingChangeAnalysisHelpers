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
using Microsoft.AspNetCore.Http.Internal;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Hosting;

namespace pgopaBreakingChangeAppV2
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            //ConfigurationRoot configurationRoot = new ConfigurationRoot(null);
            //ConfigurationSection configurationSection = new ConfigurationSection(configurationRoot, "");
            //JsonSerializerSettings serializerSettings = new JsonSerializerSettings();
            //JsonResult jr = new JsonResult("", serializerSettings);
            //jr.SerializerSettings = serializerSettings;
            log.LogInformation($"C# HTTP trigger function processed a request.{Environment.SpecialFolder.ProgramFiles}");
            DefaultHttpRequest defaultHttpRequest = new DefaultHttpRequest(new DefaultHttpContext());
            log.LogInformation($"C# HTTP trigger function  accessing defaultHttpRequest method: {defaultHttpRequest.Method}");

            RemoteAttribute remoteAttribute = new RemoteAttribute("sdfsdf");
            var castRemoteAttribute = remoteAttribute as ValidationAttribute;
            var castRemoteAttribute2 = remoteAttribute as IClientModelValidator;
            var queryParamsDictionary = new Dictionary<string, StringValues>()
            {
                {"key1", "val1" },
                { "key2", "val" }
            };
            IHostingEnvironment he = null;
            //StaticFileMiddleware sf = new StaticFileMiddleware(null, he, null, null);
            QueryCollection qc = new QueryCollection(queryParamsDictionary);

            log.LogInformation($"C# HTTP trigger function  accessing qc:{qc.Count}");

            //HtmlHelper hh = new HtmlHelper
            log.LogInformation($"{GoogleDefaults.AuthenticationScheme}");
            IDataReader reader;
            JObject x = new JObject();
            //ODataMessageWriterSettings settings = new ODataMessageWriterSettings();
            //SqlConnection connection = new SqlConnection("");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. C# HTTP trigger function  accessing defaultHttpRequest method and qc and special folder";
            //throw new Exception("test ex");
            log.LogError(new Exception("testex"), "failed");
            return new OkObjectResult(responseMessage);
        }
    }
}
