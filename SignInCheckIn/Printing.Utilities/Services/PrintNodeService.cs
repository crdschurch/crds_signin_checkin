﻿using Crossroads.Web.Common.Extensions;
using Printing.Utilities.Services.Interfaces;
using RestSharp;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Printing.Utilities.Models;
using RestSharp.Authenticators;

namespace Printing.Utilities.Services
{
    public class PrintNodeService : IPrintingService
    {
        private readonly IRestClient _printingRestClient;

        public PrintNodeService(IConfigRepository configRepository, IRestClient printingRestClient)
        {
           var printingApiKey = configRepository.GetMpConfigByKey("PrinterAPIKey").Value;

            _printingRestClient = printingRestClient;

            // printnode only asks for the api key in the username, no pw needed
            _printingRestClient.Authenticator = new HttpBasicAuthenticator(printingApiKey, string.Empty);
        }

        public int SendPrintRequest(PrintRequestDto printRequestDto)
        {
            var request = new RestRequest("/printjobs", Method.POST);
            request.SetJsonBody(printRequestDto);

            var response = _printingRestClient.Execute<int>(request);
            return response.Data;
        }
    }
}
