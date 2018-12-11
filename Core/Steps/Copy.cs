using System;
using System.IO;
using System.Text;

using BAFactory.Moira.Core.Elements;
using BAFactory.Moira.Core.Log;
using System.Threading.Tasks;

namespace BAFactory.Moira.Steps
{
    public class Copy : Step
    {
        protected bool overwrite;

        protected override bool ValidateParameters()
        {
            bool result = false;

            result = Parameters.ContainsKey("DestinationPath") && !string.IsNullOrEmpty(Parameters["DestinationPath"].Value);

            LogMessage(LogLevel.Debug, string.Format("Copy step parameters are {0}valid", result ? string.Empty : "NOT "));

            return result;
        }

        public override async Task<StepResult> Do()
        {
            StepResult result = new StepResult();
            result.ResultText = string.Empty;

            try
            {
                result.ResultText = file.CopyTo(Path.Combine(Parameters["DestinationPath"].Value, file.Name), overwrite).FullName;

                LogMessage(LogLevel.Info, string.Concat("Copied file to ", result.ResultText));
            }
            catch
            {
                LogMessage(LogLevel.Error, string.Concat("File couldn't be copied to", result.ResultText));
                return result;
            }

            result.Done = File.Exists(result.ResultText);
            return result;
        }

        public override async Task<StepResult> Undo()
        {
            //try
            //{
            //    File.Delete(Path.Combine(Parameters["DestinationPath"].Value, file.Name));
            //}
            //catch
            //{ }
            return new StepResult();
        }
    }
}
