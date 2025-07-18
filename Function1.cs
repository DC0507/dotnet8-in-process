using System;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FunctionApp2
{
    public class Function1
    {
        private readonly IConverter _converter;

        public Function1(IConverter converter)
        {
            _converter = converter;
        }

        [FunctionName("GeneratePdf")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Generating PDF using DinkToPdf...");

            string html = "<h1>Hello from Azure Functions + DinkToPdf!</h1><p>This PDF was generated dynamically.</p>";

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = new GlobalSettings
                {
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Portrait,
                    DocumentTitle = "Sample PDF"
                },
                Objects = {
                    new ObjectSettings
                    {
                        HtmlContent = html
                    }
                }
            };

            byte[] pdf = _converter.Convert(doc);

            return new FileContentResult(pdf, "application/pdf")
            {
                FileDownloadName = "generated.pdf"
            };
        }
    }
}
