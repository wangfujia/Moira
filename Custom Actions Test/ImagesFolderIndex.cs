using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BAFactory.Moira.Core.Elements;
using System.IO;
using BAFactory.Moira.FileAnalyzers;
using BAFactory.Moira.Core.Log;
using System.Threading.Tasks;

namespace BAFactory.Moira.Actions.Custom
{
    public class ImagesFolderIndex : Step
    {
        protected override bool ValidateParameters()
        {
            bool result = Parameters.ContainsKey("IndexFilePath") && !string.IsNullOrEmpty(Parameters["IndexFilePath"].Value);
            return result;
        }

        private string GetIndexFilePath()
        {
            string result = string.Empty;

            string path = file.DirectoryName;
            string folder = file.Directory.Name;

            result = string.Concat(Path.Combine(path, folder), ".Index.txt");

            return result;
        }

        private void WriteImagesIndexFile(string indexFilePath, List<FileAttribute> attribs)
        {

            foreach (FileAttribute fa in attribs)
            {
                string attribId = string.Concat(fa.Name, " (", fa.Id, "):");
                int pad = (int)((48 - attribId.Length - 1) / 8);
                string tabs = string.Empty;
                for (int i = 0; i < pad; i++)
                {
                    tabs += "\t";
                }
                File.AppendAllText(indexFilePath, string.Concat(attribId, tabs, fa.Value, Environment.NewLine));
            }
        }

        public override async Task<StepResult> Do()
        {
            StepResult result = new StepResult();
            result.ResultText = string.Empty;

            string indexFilePath = GetIndexFilePath();

            File.AppendAllText(indexFilePath, "======================================================================================" + Environment.NewLine);
            File.AppendAllText(indexFilePath, string.Concat("Image tags for: ", file.Name, Environment.NewLine));
            File.AppendAllText(indexFilePath, "======================================================================================" + Environment.NewLine);

            //TiffTagFileAnalyzer ttfa = new TiffTagFileAnalyzer();
            //List<FileAttribute> tiffAtt = ttfa.GetAllAttributes(file);
            //WriteImagesIndexFile(indexFilePath, tiffAtt);

            return result;
        }
    }
}
