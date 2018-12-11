using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

using BAFactory.Moira.Core.Elements;

namespace BAFactory.Moira.Core
{
    public static class StepsFactory
    {

        static StepsFactory()
        {
        }

        private static Assembly GetAssembly(string assemblyName, string assemblyKey, string assemblyVersion)
        {
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, String.Concat(assemblyName, ".dll"));

            Assembly assembly = Assembly.LoadFile(fullPath);

            return assembly;
        }

        public static Step CreateStep(StepAssemblyInfo a, Parameters p)
        {
            Assembly loadedAssembly = GetAssembly(a.Assembly, a.PublicKeyToken, a.Version);
            Step s = (Step)loadedAssembly.CreateInstance(a.Class);
            s.ParametersTemplate = p;
            return s;
        }
     }
}
