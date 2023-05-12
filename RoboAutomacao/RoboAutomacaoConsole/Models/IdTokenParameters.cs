using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboAutomacaoConsole.Models
{
    public class IdTokenParameters
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Idp { get; set; } = string.Empty;
        public IList<string> RequestedScopes { get; set; } = new List<string>();
        public Document Document { get; set; } = new();
    }
}
