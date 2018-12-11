using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BAFactory.Moira.Core
{
    public abstract class ExecutionResult
    {
        public bool Done { get; set; }
        public bool Undone { get; set; }
        public String ResultText { get; set; }
    }
}
