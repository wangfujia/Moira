using System;
using System.IO;
using System.Reflection;
using BAFactory.Moira.Core.Elements;
using System.Threading.Tasks;

namespace BAFactory.Moira.FileAnalyzers
{
    public class AttributesFileAnalyzer : IFileAnalyzer
    {
        FileInfo fi;

        public void Configure(FileInfo f)
        {
            fi = f;
        }
        public string GetAttribute(FileAttribute a, string format)
        {
            string result = string.Empty;

            PropertyInfo pi = null;
            try
            {
                pi = fi.GetType().GetProperty(a.Name) as PropertyInfo;
            }
            catch
            {
            }

            if (pi == null)
            {
                return string.Empty;
            }

            result = pi.GetValue(fi, null).ToString();

            if (a.Type.Equals("System.DateTime"))
            {
                DateTime date = DateTime.Parse(result);
                result = date.ToString(format);
            }
            return result;
        }
    }
}
