using System;
using System.IO;
using System.Text;
using BAFactory.Fx.Utilities.ImageProcessing;
using BAFactory.Moira.Core.Elements;
using BAFactory.Fx.FileTags.Exif;
using BAFactory.Fx.FileTags.Exif.IFD;

namespace BAFactory.Moira.FileAnalyzers
{
    public class ExifTagFileAnalyzer : IFileAnalyzer
    {
        ImageTagExtractor imageTagExtractor;
        FileInfo fileInfo;

        public void Configure(FileInfo f)
        {
            fileInfo = f;
        }

        public string GetAttribute(FileAttribute a, string format)
        {
            string result = string.Empty;

            imageTagExtractor = new ImageTagExtractor(fileInfo.FullName);

            int tag = (int)Enum.Parse(typeof(IFDTagCode), a.Name);

            var field = imageTagExtractor.GetTiffTag(fileInfo.FullName, tag);

            if (field == null || string.IsNullOrEmpty(field.Text))
            {
                return result;
            }

            if (a.Type.Equals("System.DateTime"))
            {
                DateTime date = DateTime.MinValue;
                DateTime time = DateTime.MinValue;

                string[] dateParts = field.Text.Split(' ');
                string replaced = dateParts[0].Replace(':', '/');
                DateTime.TryParse(replaced, out date);

                if (date.Year.Equals(DateTime.MinValue.Year))
                    return string.Empty;

                DateTime.TryParse(dateParts[1], out time);

                DateTime recalculated = date.AddHours(time.Hour).AddMinutes(time.Minute).AddSeconds(time.Second);

                result = recalculated.ToString(format);
            }
            else
            {
                result = field.Text;
            }

            return result;
        }
    }
}