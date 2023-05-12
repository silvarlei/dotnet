using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboAutomacaoConsole.Models
{
    public class RedisAppItem
    {
        public string Chave { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
        public string Expiracao { get; set; } = string.Empty;
    }
}
