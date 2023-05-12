using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboAutomacaoConsole.Models
{
    public class UserInfoParameters
    {
        [JsonProperty("sub")]
        public Guid UserId { get; set; } = Guid.NewGuid();

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("document")]
        public Document Document { get; set; } = new();
    }

    public class Document
    {
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("number")]
        public string Number { get; set; } = string.Empty;
    }
}
