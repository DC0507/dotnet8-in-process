/*using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FunctionApp2
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}*/
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;


[assembly: FunctionsStartup(typeof(Cms.Functions.Startup))]

namespace Cms.Functions
{
    public class Startup : FunctionsStartup
    {
        private readonly static string[] SoLibs = new string[] {
             "libbsd.so.0",
             "libcrypto.so.1",
             "libfontconfig.so.1",
             "libfreetype.so.6",
             "libjpeg.so.8",
             "libpng16.so.16",
             "libssl.so.1",
             "libuuid.so.1",
             "libX11.so.6",
             "libXau.so.6",
             "libxcb.so.1",
             "libXdmcp.so.6",
             "libXrender.so.1"
            };
        static Startup()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
                foreach (var so in SoLibs)
                {
                    NativeLibrary.Load(Path.Combine(appDir, so));
                }
            }
        }
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            builder.ConfigurationBuilder
                .AddJsonFile(Path.Combine(builder.GetContext().ApplicationRootPath, $"local.settings.json"), optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }
        public override void Configure(IFunctionsHostBuilder builder)
        {
            //var config = new ConfigurationBuilder()
            //    .SetBasePath(Directory.GetCurrentDirectory())
            //    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            //    .AddEnvironmentVariables()
            //    .Build();
            builder.Services.AddMemoryCache();
            builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
            builder.Services.AddHttpClient(Consts.CmsApiClientName, httpClient =>
            {
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.BaseAddress = new Uri(Environment.GetEnvironmentVariable(Consts.CmsApiBaseUrlKey)!.TrimEnd('/') + '/');
            });

            // When using System.Text.Json (Preferred)
            builder.Services.Configure<JsonSerializerOptions>(o =>
            {
                o.PropertyNameCaseInsensitive = true;
            });
            //When using Newtonsoft.Json for legacy reasons
            builder.Services.Configure<JsonSerializerSettings>(s =>
            {
                s.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });
        }
    }
}
