using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using BAFactory.Moira.Core.Elements;
using System.Threading.Tasks;

namespace BAFactory.Moira.Core
{
    public class ExecutionManager
    {
        [FlagsAttribute]
        public enum ExecutionFlags
        {
            None = 0,
            RevertOnFail = 1,
            UndoOnly = 2,
        }

        public async Task<JobResult> ExecuteJob(FileInfo fi, List<StepsGroup> sg)
        {
            JobResult result = new JobResult();

            foreach (StepsGroup g in sg)
            {
                result = await ExecuteStepsGroup(fi, g);

                switch (g.BreakCondition)
                {
                    case BreakCondition.Always:
                        return result;
                    case BreakCondition.OnError:
                        if (!result.Done)
                        {
                            return result;
                        }
                        break;
                    case BreakCondition.OnSuccess:
                        if (result.Done)
                        {
                            return result;
                        }
                        break;
                }
            }
            return result;
        }

        public async Task<JobResult> ExecuteStepsGroup(FileInfo f, StepsGroup g)
        {
            JobResult result = new JobResult();
            string resultText = string.Empty;
            bool loopStatus = false;

            foreach (Step s in g)
            {
                s.LastCommandReport = resultText;
                result = await ExecuteStep(f, s);

                switch (s.BreakCondition)
                {
                    case BreakCondition.Always:
                        return result;
                    case BreakCondition.OnError:
                        if (!result.Done)
                        {
                            return result;
                        }
                        break;
                    case BreakCondition.OnSuccess:
                        if (result.Done)
                        {
                            return result;
                        }
                        break;
                }
                resultText = result.ResultText;
                loopStatus &= result.Done;
            }
            result.Done = loopStatus;
            return result;
        }

        public async Task<JobResult> ExecuteStep(FileInfo f, Step s)
        {
            JobResult result = new JobResult();

            if (!await s.ExtendParameters(f))
            {
                return result;
            }

            StepResult step = await s.Do();
            result.Done = step.Done;
            result.ResultText = step.ResultText;

            // TODO: Add execution flags
            //if ((flags & ExecutionFlags.UndoOnly) != ExecutionFlags.UndoOnly)
            //{
            //    done = s.Do(out resultText);
            //}
            //if ((flags & ExecutionFlags.UndoOnly) == ExecutionFlags.UndoOnly || 
            //    !done)
            //{
            //    undone = s.Undo();
            //}
            //if ((flags & ExecutionFlags.UndoOnly) == ExecutionFlags.UndoOnly)
            //{
            //    done = undone;
            //}

            return result;
        }

        private async Task<Parameters> SetLastResultParameters(Parameters parameters, string value)
        {
            Parameters result = parameters;

            await Task.Run(() =>
            {
                foreach (Parameter p in parameters.Values)
                {
                    if (p.IsFromLastResult)
                    {
                        p.Value = value;
                    }
                }
            });

            return result;
        }
    }
}
