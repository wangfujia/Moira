using System;
using System.IO;
using System.Reflection;
using BAFactory.Moira.Core.Elements;
using BAFactory.Moira.FileAnalyzers;

namespace BAFactory.Moira.Core
{
    internal class FileAnalyzersFactory : BaseTypeFactory<IFileAnalyzer>
    {
        internal IFileAnalyzer InstantiateFileAnalyzer(AssemblyInformation ai)
        {
            return base.InstantiateClass(ai);   
        }
    }
}
