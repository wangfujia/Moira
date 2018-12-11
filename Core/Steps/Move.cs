using System;
using System.IO;
using System.Text;

using BAFactory.Moira.Core.Elements;
using BAFactory.Moira.Core.Log;
using System.Threading.Tasks;

namespace BAFactory.Moira.Steps
{
    public class Move : Step
    {
        protected bool overwrite;

        /// <summary>
        /// Creates a Copy Operation. 
        /// </summary>
        /// <param name="p">Should provide: [source path], [file name], [destination path]</param>
        /// <remarks>The destination path should already exists</remarks>
        public Move()
        { }

        protected override bool ValidateParameters()
        {
            bool result = false;

            result = Parameters.ContainsKey("DestinationPath") && !string.IsNullOrEmpty(Parameters["DestinationPath"].Value);

            LogMessage(LogLevel.Debug, string.Format("Move step parameters are {0}valid", result ? string.Empty : "NOT "));

            return result;
        }

        public override async Task<StepResult> Do()
        {
            string newPath;

            StepResult result = new StepResult();

            newPath = Path.Combine(Parameters["DestinationPath"].Value, file.Name);
            try
            {
                await Task.Run(() => { file.MoveTo(newPath); });

                result.Done = true;
                result.ResultText = newPath;

                LogMessage(LogLevel.Info, string.Concat("Moved file to ", newPath));
            }
            catch
            {
                LogMessage(LogLevel.Error, string.Concat("File couldn't be moved to ", newPath));
            }

            result.Done = File.Exists(newPath);

            return result;
        }

        public override async Task<StepResult> Undo()
        {
            //try
            //{
            //    //File.Delete(Path.Combine(parameters["DestinationPath"].Value, file.Name));
            //}
            //catch
            //{ }
            return new StepResult();
        }
    }
}
