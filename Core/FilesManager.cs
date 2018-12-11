using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;

using BAFactory.Moira.Core.Elements;

namespace BAFactory.Moira.Core
{
    static class FilesManager
    {
        public static FileInfo[] GetMatchingFiles(string path, FilePattern pattern)
        {
            FileInfo[] result = null;

            if (pattern.IsRegEx)
            {
                result = GetRegexMatchingFiles(path, pattern.Pattern);
            }
            else
            {
                result = GetStringMatchingFiles(path, pattern.Pattern);
            }

            return result;
        }

        private static FileInfo[] GetRegexMatchingFiles(string path, string pattern)
        {
            List<FileInfo> result = null;

            string[] fileNames = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
            foreach (string fileName in fileNames)
            {
                Regex rx = new Regex(pattern, RegexOptions.IgnoreCase);
                if (!rx.IsMatch(fileName))
                {
                    continue;
                }

                FileInfo fi = new FileInfo(Path.Combine(path, pattern));
                if (fi != null && fi.Exists)
                {
                    result.Add(fi);
                }
            }

            return result.ToArray();

        }

        private static FileInfo[] GetStringMatchingFiles(string path, string pattern)
        {
            List<FileInfo> result = new List<FileInfo>();

            string[] fileNames = Directory.GetFiles(path, pattern, SearchOption.TopDirectoryOnly);
            foreach (string fileName in fileNames)
            {
                FileInfo fi = new FileInfo(Path.Combine(path, fileName));
                if (fi != null && fi.Exists)
                {
                    result.Add(fi);
                }
            }

            return result.ToArray();

        }
    }
}
