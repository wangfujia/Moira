using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using BAFactory.Moira.Core.Elements;
using System.IO;

namespace BAFactory.Moira.Core.Log
{
    internal class LogProviderFactory : BaseTypeFactory<ILogProvider>
    {
        internal ILogProvider InstantiateLogProvider(AssemblyInformation ai)
        {
            ILogProvider lp = base.InstantiateClass(ai);
            return lp;
        }
    }
}
