using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Application.Core.Interfaces;
using JWTSecureApi.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Data;
using System.Security.Cryptography.X509Certificates;
using Application.Core.Models;
using Microsoft.Extensions.Options;

namespace JWTSecureApi.Endpoints
{

    public class JWTSecureEndpoints
    {
        private readonly ILogger<JWTSecureEndpoints> _logger;
        private readonly ITokenValidator _tokenValidator;
        private readonly AzureAdSettings _azureAdSettings;
        private static IConfiguration configuration;

        public JWTSecureEndpoints(ITokenValidator tokenValidator, IOptions<AzureAdSettings> adSettings, ILogger<JWTSecureEndpoints> logger)
        {
            _tokenValidator = tokenValidator;
            _azureAdSettings = adSettings.Value;
            _logger = logger;
        }


        [FunctionName("JWTSecureEndpoints")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ExecutionContext context)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            var claimsPrincipal = await _tokenValidator.ValidateTokenAsync(req, _azureAdSettings.Scope);

            if (!_tokenValidator.HasRightRolesAndScope(claimsPrincipal, _azureAdSettings.ScopeName, _azureAdSettings.Roles))
            {
                return new UnauthorizedResult();
            }

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            // alternative way to get configuration settings here as an example
            configuration = GetAzureADConfiguration(context);

            return new OkObjectResult(responseMessage);
        }

        // alternative way to get configuraion from local.settings.json outside of the values property
        private static IConfiguration GetAzureADConfiguration(ExecutionContext context)
        {
            var config = new ConfigurationBuilder()
                    .SetBasePath(context.FunctionAppDirectory)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

            return config.GetSection("AzureAd");
        }
    }
}
