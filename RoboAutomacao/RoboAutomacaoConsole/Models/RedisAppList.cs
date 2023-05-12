using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboAutomacaoConsole.Models
{
    public class RedisAppList
    {
        public List<RedisAppItem> Data { get; set; }
        public RedisAppList(List<RedisAppItem> data) 
        {
            Data = data;
        }
    }
}
