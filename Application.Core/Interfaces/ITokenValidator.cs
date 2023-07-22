using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Application.Core.Interfaces
{
    public interface ITokenValidator
    {
        void GetJwtFromHeader(HttpRequest req);
        bool HasRightRolesAndScope(ClaimsPrincipal claimsPrincipal, string scopeName, string[]? roles);
        Task<ClaimsPrincipal> ValidateTokenAsync(HttpRequest req, string audience = "");
    }
}
