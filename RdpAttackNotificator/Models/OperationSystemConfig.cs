using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RdpAttackNotificator.Models
{
    public class OperationSystemConfig
    {
        public Int32 InstanceId { get; set; }

        public String Version { get; set; }

        public String Source { get; set; }

        public List<Regex> SearchEngines { get; set; }
    }
}
