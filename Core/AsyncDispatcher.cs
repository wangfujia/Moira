using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using System.Text;

using BAFactory.Moira.Core.Elements;
using BAFactory.Moira.Core.Log;
using System.Threading.Tasks;

namespace BAFactory.Moira.Core
{
    public class AsyncDispatcher : Dispatcher
    {
        public override bool IsEnabled
        {
            get { return MainTimer.Enabled; }
        }
        public AsyncDispatcher()
        {
            InitializeComponents();
        }
        private void InitializeComponents()
        {
            MainTimer = new System.Timers.Timer();
            MainTimer.Interval = 1000;
        }
        protected override async void RunOnStartup()
        {
            await Configuration.LogWriter.LogMessageAsync(LogLevel.Debug, "Running tasks on Startup");
            foreach (Timetable.Entry entry in Configuration.TasksLists)
            {
                if (!entry.Task.Enabled || !entry.Task.RunOnStartUp || entry.IsExecuting)
                {
                    continue;
                }

                Dispatch(entry);
            }
        }
        protected override async void AttendTimer(object sender, ElapsedEventArgs e)
        {
            await Configuration.LogWriter.LogMessageAsync(LogLevel.Info, "Running tasks on schedule");
            foreach (Timetable.Entry entry in Configuration.TasksLists)
            {
                bool isOnTime = await CheckIsOnTime(entry);
                if (isOnTime)
                {
                    Dispatch(entry);
                }
            }
        }
        private async Task<bool> CheckIsOnTime(Timetable.Entry e)
        {
            bool isOnTime = (ulong)DateTime.Now.Ticks >= e.NextRun;
            await Configuration.LogWriter.LogMessageAsync(LogLevel.Debug, string.Format("Task {0} is {1}on time", e.Task.Id, isOnTime ? "NOT " : string.Empty));
            await Configuration.LogWriter.LogMessageAsync(LogLevel.Debug, string.Format("Task {0} is {1}running", e.Task.Id, !e.IsExecuting ? "NOT " : string.Empty));
            await Configuration.LogWriter.LogMessageAsync(LogLevel.Debug, string.Format("Task {0} will {1}run now", e.Task.Id, (!(e.IsExecuting && isOnTime)) ? "NOT " : string.Empty));


            return (!e.IsExecuting && (ulong)DateTime.Now.Ticks >= e.NextRun);
        }
        private async void Dispatch(Timetable.Entry e)
        {
            await Configuration.LogWriter.LogMessageAsync(LogLevel.Debug, string.Concat("Spawning execution of task with Id: ", e.Task.Id));

            if (e == null)
            {
                return;
            }

            UpdateTimeTable(e, true);

            try
            {
                bool success = false;

                success = await e.Task.ExecuteAsync();

                if (!success && !string.IsNullOrEmpty(e.Task.Error))
                {
                    await Configuration.LogWriter.LogMessageAsync(LogLevel.Error, e.Task.Error);
                    e.Task.Revert();
                }
            }
            finally
            {
                UpdateTimeTable(e, false);
            }
        }
        private static void UpdateTimeTable(Timetable.Entry e, bool executing)
        {
            ulong currentTicks = 0;
            e.IsExecuting = executing;

            currentTicks = (ulong)DateTime.Now.Ticks;

            if (executing)
                e.NextRun = currentTicks + e.Task.Interval;
        }
        private void CalculateTimeTable()
        { }
        private void ResetTimeTable()
        { }
    }
}
