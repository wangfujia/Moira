using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

using BAFactory.Moira.Core;

using BAFactory.Moira.Core.Elements;
using System.Threading.Tasks;

namespace BAFactory.Moira.Actions.Custom
{
    public class BulkCopy : Step
    {
        public BulkCopy()
        { }

        protected override bool ValidateParameters()
        {
            if (!ParametersTemplate.ContainsKey("DestinationPath"))
            {
                return false;
            }
            return true;
        }

        public override async Task<StepResult> Do()
        {
            StepResult result = new StepResult();
            result.ResultText = string.Empty;

            try
            {
                file.CopyTo(Path.Combine(ParametersTemplate["DestinationPath"].Value, file.Name));
                result.ResultText = Path.Combine(ParametersTemplate["DestinationPath"].Value, file.Name);
                result.Done = true;
            }
            catch
            {
                return result;
            }

            return result;
        }

        public override async Task<StepResult> Undo()
        {
            return new StepResult();
        }
    }
}
