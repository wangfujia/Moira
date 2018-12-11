using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BAFactory.Moira.Core.Elements;
using BAFactory.Moira.Core.Log;
using System.Threading.Tasks;

namespace BAFactory.Moira.Core
{
    public class StepsGroup : List<Step>
    {
        public string Id{ get; set; }
        public bool Enabled {get; set;}
        public BreakCondition BreakCondition { get; set; }

        public Func<LogLevel, string, Task> LogMessageAction { get; internal set; }

        public StepsGroup()
            : base()
        { }
    }
}
