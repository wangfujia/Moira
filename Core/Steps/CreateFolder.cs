using System;
using System.IO;
using System.Text;

using BAFactory.Moira.Core.Elements;
using BAFactory.Moira.Core.Log;
using System.Threading.Tasks;

namespace BAFactory.Moira.Steps
{
    public class CreateFolder : Step
    {
        protected bool overwrite;

        /// <summary>
        /// Creates a Copy Operation. 
        /// </summary>
        /// <param name="p">Should provide: [source path], [file name], [destination path]</param>
        /// <remarks>The destination path should already exists</remarks>
        public CreateFolder()
        { }

        protected override bool ValidateParameters()
        {
            bool result = false;
        
            result = Parameters.ContainsKey("FullDirectoryName") && !string.IsNullOrEmpty(Parameters["FullDirectoryName"].Value);

            LogMessage(LogLevel.Debug, string.Format("CreateFolder step parameters are {0}valid", result ? string.Empty : "NOT "));

            return result;
        }

        public override async Task<StepResult> Do()
        {
            StepResult result = new StepResult();

            DirectoryInfo di;

            string composeDirName = Parameters["FullDirectoryName"].Value;

            try
            {
                di = Directory.CreateDirectory(composeDirName);

                result.ResultText = di.FullName;
                result.Done = true;
                LogMessage(LogLevel.Info, string.Concat("Created folder ", result.ResultText));
            }
            catch
            {
                LogMessage(LogLevel.Error, string.Concat("Could not create folder ", result.ResultText));
                result.Done = false;
            }

            return result;
        }

        public override async Task<StepResult> Undo()
        {
            try
            {
                Directory.Delete(Parameters["FullDirectoryName"].Value);

                LogMessage(LogLevel.Info, "Folder was deleted");
            }
            catch
            { }

            return new StepResult();
        }
    }
}
