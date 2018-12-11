using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;

using System.Security.Policy;
using System.Security.Permissions;
using BAFactory.Moira.Core.Log;
using System.Threading.Tasks;

namespace BAFactory.Moira.Core.Elements
{
    public abstract class Step
    {
        protected FileInfo file;

        public Func<LogLevel, string, Task> LogMessageAction { get; internal set; }
        public Parameters ParametersTemplate { get; internal set; }
        public Parameters Parameters { get; set; }
        public string LastCommandReport { get; set; }
        public BreakCondition BreakCondition { get; set; }
        protected Step()
        {
            LastCommandReport = string.Empty;
        }
        public string GetParameterValue(string parameterName)
        {
            string result = string.Empty;

            if (!ParametersTemplate.ContainsKey(parameterName))
            {
                return result;
            }

            if (ParametersTemplate[parameterName].IsFromLastResult)
            {
                return result;
            }

            result = ParametersTemplate[parameterName].Value;

            return result;
        }
        public async Task<bool> ExtendParameters(FileInfo f)
        {
            bool result = await Task.Run(() => ExpandParameters(f));
            if (!ExpandParameters(f))
            {
                return false;
            }
            this.file = f;
            
            return ValidateParameters();
        }
        public abstract Task<StepResult> Do();
        public virtual async Task<StepResult> Undo()
        {
            return new StepResult();
        }
        protected virtual bool ValidateParameters()
        {
            return true; 
        }
        protected bool ExpandParameters(FileInfo f)
        {
            this.Parameters = (Parameters)ParametersTemplate.Clone();

            foreach (string key in this.Parameters.Keys)
            {
                Parameter param = (Parameter)Parameters[key];

                if (param.IsFromLastResult)
                {
                    if (string.IsNullOrEmpty(LastCommandReport))
                    {
                        return false;
                    }
                    else
                    {
                        param.Value = LastCommandReport;
                        return true;
                    }
                }

                if (!string.IsNullOrEmpty(param.Attribute.Name))
                {
                    if (!Parameter.ExpandParameter(f, ref param))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        protected async void LogMessage(LogLevel l, string m)
        {
            if (LogMessageAction != null)
            {
                await LogMessageAction(l, m);
            }
        }
    }
}
