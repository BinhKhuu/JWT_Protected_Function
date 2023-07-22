using Application.Core.Interfaces;
using Application.Core.Models;
using JWTSecureApi.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

[assembly: FunctionsStartup(typeof(JWTSecureApi.Startup))]

namespace JWTSecureApi
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {

            builder.Services.AddScoped<ITokenValidator, TokenValidator>();
            builder.Services.AddOptions<AzureAdSettings>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection("AzureAd").Bind(settings);
                });
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            FunctionsHostBuilderContext context = builder.GetContext();

            builder.ConfigurationBuilder
                .SetBasePath(context.ApplicationRootPath)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, "appsettings.json"), optional: true, reloadOnChange: true)
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"appsettings.{context.EnvironmentName}.json"), optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
        }
    }
}
