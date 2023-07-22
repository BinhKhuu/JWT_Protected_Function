using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.Models
{
    public class AzureAdSettings
    {
        public string? TenantId { get; set; }
        public string? ClientId { get; set; }
        public string? Scope { get; set; }
        public string? ScopeName { get; set; }
        public string[]? Roles { get; set; }
    }
}
