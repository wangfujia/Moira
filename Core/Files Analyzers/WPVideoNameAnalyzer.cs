using BAFactory.Moira.Core.Elements;
using BAFactory.Moira.FileAnalyzers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BAFactory.Moira.FileAnalyzers
{
    public class WpVideoNameAnalyzer : IFileAnalyzer
    {
        FileInfo fi;
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

            var name = pi.GetValue(fi, null).ToString();

            Regex rx = new Regex(@"WP_(\d{8})_.*\.mp4");

            var match = rx.Match(name);

            if (!match.Success) return result;

            var matchValue = match.Groups[1].Value;

            DateTime tempOut = DateTime.MinValue;

            if (!DateTime.TryParseExact(matchValue, "yyyyMMdd", new CultureInfo("en-US"), DateTimeStyles.None, out tempOut))
            {
                return result;
            }

            return tempOut.ToString(format);
        }

        public void Configure(FileInfo f)
        {
            fi = f;
        }
    }
}
