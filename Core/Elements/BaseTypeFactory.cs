using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using BAFactory.Moira.Core.Log;

namespace BAFactory.Moira.Core.Elements
{
    internal abstract class BaseTypeFactory<T>
    {
        protected T InstantiateClass(AssemblyInformation a)
        {
            Assembly loadedAssembly = GetAssembly(a.Assembly, a.PublicKeyToken, a.Version);
            T analyzer = (T)loadedAssembly.CreateInstance(a.Class);
            return analyzer;
        }

        protected Assembly GetAssembly(string assemblyName, string assemblyKey, string assemblyVersion)
        {
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, String.Concat(assemblyName, ".dll"));
            Assembly assembly = Assembly.LoadFile(fullPath);
            return assembly;
        }
    }
}
