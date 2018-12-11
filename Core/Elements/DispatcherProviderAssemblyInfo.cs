using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace BAFactory.Moira.Core.Elements
{
    public class DispatcherProviderAssemblyInfo : AssemblyInformation
    {
        public DispatcherProviderAssemblyInfo(string className, string assembly, string publicToken, string version)
            : base(string.Empty, className, assembly, publicToken, string.Empty, version)
        {
        }
    }
}
