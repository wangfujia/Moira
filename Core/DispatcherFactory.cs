using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using BAFactory.Moira.Core.Elements;
using System.IO;

namespace BAFactory.Moira.Core.Log
{
    internal class DispatcherFactory : BaseTypeFactory<Dispatcher>
    {
        internal Dispatcher InstantiateDispatcher(AssemblyInformation ai)
        {
            Dispatcher d = base.InstantiateClass(ai);
            return d;
        }
    }
}
