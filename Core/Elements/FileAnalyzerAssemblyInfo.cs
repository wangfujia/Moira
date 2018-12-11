using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace BAFactory.Moira.Core.Elements
{
    public class FileAnalyzerAssemblyInfo : AssemblyInformation
    {
        public FileAnalyzerAssemblyInfo(string className, string assembly, string publicToken, string version)
            : this(string.Empty, className, assembly, publicToken, string.Empty, version)
        {
        }
        public FileAnalyzerAssemblyInfo(string id, string className, string assembly, string publicToken, string culture, string version)
            : base(className, assembly, publicToken, version)
        {
        }
    }
}
