using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace BAFactory.Moira.Core.Elements
{
    public class StepAssemblyInfo : AssemblyInformation
    {
        public StepAssemblyInfo(string className, string assembly, string publicToken, string version)
            : this(string.Empty, className, assembly, publicToken, string.Empty, version)
        {
        }
        public StepAssemblyInfo(string id, string className, string assembly, string publicToken, string culture, string version)
            : base(className, assembly, publicToken, version)
        {
        }
    }
}
