using System.IO;
using BAFactory.Moira.Core.Elements;
using System.Threading.Tasks;

namespace BAFactory.Moira.FileAnalyzers
{
    public interface IFileAnalyzer
    {
        string GetAttribute(FileAttribute a, string format);
        void Configure(FileInfo f);
    }
}
