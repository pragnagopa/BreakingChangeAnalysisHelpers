using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authentication.Google;
using System.Data;
using Newtonsoft.Json.Linq;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using Microsoft.Extensions.Caching.SqlServer;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore;
using Microsoft.Azure.Documents.SystemFunctions;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.MiddlewareAnalysis;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.AspNetCore.Owin;
using static Microsoft.AspNetCore.Owin.OwinEnvironment;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using Microsoft.AspNetCore.SpaServices;
using Microsoft.IdentityModel.Protocols.WsFederation;
using Microsoft.IdentityModel.Tokens.Saml;
using Microsoft.IdentityModel.Xml;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Hosting;

namespace PgopaBreakingChangeFunctionApp
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed a request.{Process.GetCurrentProcess().Id}");

            //ConfigurationRoot configurationRoot = new ConfigurationRoot(null);
            //ConfigurationSection configurationSection = new ConfigurationSection(configurationRoot, "");

            //JsonSerializerSettings serializerSettings = new JsonSerializerSettings();
            //JsonResult jr = new JsonResult("", serializerSettings);
            //jr.SerializerSettings = serializerSettings;

            RemoteAttribute remoteAttribute = new RemoteAttribute("sdfsdf");
            var castRemoteAttribute = remoteAttribute as ValidationAttribute;
            var castRemoteAttribute2 = remoteAttribute as IClientModelValidator;
            ClientModelValidationContext context = null;
            castRemoteAttribute2.AddValidation(context);
            IHostingEnvironment he = null;
            //StaticFileMiddleware sf = new StaticFileMiddleware(null, he, null, null);

            //DefaultHttpRequest defaultHttpRequest = new DefaultHttpRequest(new DefaultHttpContext());

            //FacebookOptions fbOptions = new FacebookOptions();
            //DatabaseErrorPageMiddleware databaseErrorPageMiddleware = new DatabaseErrorPageMiddleware(null, null, null);
            //databaseErrorPageMiddleware.IsArray();
            //log.LogInformation($"{fbOptions.AppId}");
            //GoogleOptions googleOptions = new GoogleOptions();
            //log.LogInformation($"{googleOptions.ClientId}");
            //UIFrameworkAttribute uIFrameworkAttribute = new UIFrameworkAttribute("");
            //AnalysisMiddleware analysisMiddleware = new AnalysisMiddleware(null, null, ""); 
            //NodeServicesOptions nodeServicesOptions = new NodeServicesOptions(null);
            //FeatureMap featureMap = new OwinEnvironment.FeatureMap(null, null);

            //var buffer = Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal.KestrelMemoryPool.Create();
            //log.LogInformation($"{ListenType.FileHandle}");
            //SpaOptions spaOptions = new SpaOptions();
            //log.LogInformation($"{spaOptions.DefaultPage}");
            //WsFederationConfiguration wsFederationConfiguration = new WsFederationConfiguration();
            //log.LogInformation($"{wsFederationConfiguration.GetType()}");
            //SamlAction samlAction = new SamlAction("");
            //EnvelopedSignatureReader envelopedSignatureReader = new EnvelopedSignatureReader(null);
            //var webSocket = System.Net.WebSockets.WebSocketProtocol.CreateFromStream(null, true, null,  TimeSpan.Zero);
            IDataReader reader;
            JObject x = new JObject();
           // SqlServerCacheOptions sqlServerCacheOptions = new SqlServerCacheOptions();
            //log.LogInformation($"sqlServerCacheOption:{sqlServerCacheOptions.SystemClock}");
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
            return new OkObjectResult(responseMessage);
        }
    }
}
