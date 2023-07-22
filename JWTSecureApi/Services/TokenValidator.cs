using Application.Core.Interfaces;
using Application.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JWTSecureApi.Services
{
    public class TokenValidator : ITokenValidator
    {
        private ILogger _logger;
        private const string scopeType = @"http://schemas.microsoft.com/identity/claims/scope";
        private string token;
        private readonly AzureAdSettings _azureAdSettings;
        public string Token
        {
            get { return token; }
            private set { token = value; }
        }
        public TokenValidator(ILogger logger, IOptions<AzureAdSettings> adSettings)
        {
            _logger = logger;
            _azureAdSettings = adSettings.Value;
        }

        public void GetJwtFromHeader(HttpRequest req)
        {
            var authorizationHeader = req.Headers?["Authorization"];
            string[] parts = authorizationHeader?.ToString().Split(null) ?? new string[0];
            Token = (parts.Length == 2 && parts[0].Equals("Bearer")) ? parts[1] : string.Empty;
        }

        public async Task<ClaimsPrincipal> ValidateTokenAsync(HttpRequest req, string audience = "")
        {
            GetJwtFromHeader(req);
            if (string.IsNullOrEmpty(Token))
            {
                return null;
            }

            var jwtHandler = new JwtSecurityTokenHandler();

            // OIDConfig needed to validate the JwtSecurityToken
            // https://learn.microsoft.com/en-us/azure/active-directory/develop/v2-protocols-oidc
            var ConfigManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                     $"https://login.microsoftonline.com/{_azureAdSettings.TenantId}/v2.0/.well-known/openid-configuration", 
                     new OpenIdConnectConfigurationRetriever());
            var OIDconfig = await ConfigManager.GetConfigurationAsync();

            var tokenValidator = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidAudiences = new string[] { _azureAdSettings.ClientId, audience },
                ValidateAudience = true,
                IssuerSigningKeys = OIDconfig.SigningKeys,
                ValidIssuer = OIDconfig.Issuer
            };

            // You can decrypt a token like this but not validate it
            var test = new JwtSecurityToken(token);
            Console.WriteLine("email => " + test.Claims.First(c => c.Type == "roles").Value);


            try
            {
                SecurityToken securityToken;
                var claimsPrincipal = tokenValidator.ValidateToken(token, validationParameters, out securityToken);
                return claimsPrincipal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            return null;
        }

        public bool HasRightRolesAndScope(ClaimsPrincipal claimsPrincipal, string scopeName, string[] roles = null)
        {
            bool isInRole = false;
            if (claimsPrincipal == null)
            {
                return false;
            }

            if (roles != null)
            {
                foreach (var role in roles)
                {
                    if (claimsPrincipal.IsInRole(role))
                    {
                        isInRole = true;
                    }
                }
            }

            if (!isInRole)
            {
                return false;
            }

            var scopeClaim = claimsPrincipal.HasClaim(x => x.Type == scopeType)
                ? claimsPrincipal.Claims.First(x => x.Type == scopeType).Value
                : string.Empty;

            if (string.IsNullOrEmpty(scopeClaim))
            {
                return false;
            }

            if (!scopeClaim.Equals(scopeName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

    }
}
