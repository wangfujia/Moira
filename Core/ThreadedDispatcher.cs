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
    public class ThreadedDispatcher : Dispatcher
    {
        public override bool IsEnabled
        {
            get { return MainTimer.Enabled; }
        }

        public ThreadedDispatcher()
        {
            InitializeComponents();

            RunOnStartup();

            StartTimer();

        }

        private void InitializeComponents()
        {
            MainTimer = new System.Timers.Timer();
            MainTimer.Interval = 1000;
        }

        protected override void RunOnStartup()
        {
            Configuration.LogWriter.LogMessage(LogLevel.Debug, "Running tasks on Startup");
            foreach (Timetable.Entry entry in Configuration.TasksLists)
            {
                if (!entry.Task.Enabled || !entry.Task.RunOnStartUp || entry.IsExecuting)
                {
                    continue;
                }

                Dispatch(entry);
            }
        }

        protected override void AttendTimer(object sender, ElapsedEventArgs e)
        {
            Configuration.LogWriter.LogMessage(LogLevel.Info, "Running tasks on schedule");
            foreach (Timetable.Entry entry in Configuration.TasksLists)
            {
                bool isOnTime = CheckIsOnTime(entry);
                if (isOnTime)
                {
                    Dispatch(entry);
                }
            }
        }

        private bool CheckIsOnTime(Timetable.Entry e)
        {
            bool isOnTime = (ulong)DateTime.Now.Ticks >= e.NextRun;
            Configuration.LogWriter.LogMessage(LogLevel.Debug, string.Format("Task {0} is {1}on time", e.Task.Id, isOnTime ? "NOT " : string.Empty));
            Configuration.LogWriter.LogMessage(LogLevel.Debug, string.Format("Task {0} is {1}running", e.Task.Id, !e.IsExecuting ? "NOT " : string.Empty));
            Configuration.LogWriter.LogMessage(LogLevel.Debug, string.Format("Task {0} will {1}run now", e.Task.Id, (!(e.IsExecuting && isOnTime)) ? "NOT " : string.Empty));


            return (!e.IsExecuting && (ulong)DateTime.Now.Ticks >= e.NextRun);
        }

        private void Dispatch(Timetable.Entry e)
        {
            Configuration.LogWriter.LogMessage(LogLevel.Debug, string.Concat("Spawning execution of task with Id: ", e.Task.Id));
            SpawnExecution(e);
        }

        private static void SpawnExecution(Timetable.Entry e)
        {
            Thread thrd = new Thread(ExecuteTask);
            thrd.Start(e);
        }

        private static void ExecuteTask(object o)
        {
            Timetable.Entry e = o as Timetable.Entry;

            if (e == null)
            {
                return;
            }

            UpdateTimeTable(e, true);
            try
            {
                bool success = false;

                success = true;
                while (e.Task.ExecuteAsync().Status == TaskStatus.Running)
                {
                    Thread.Sleep(100);
                }

                if (!success && !string.IsNullOrEmpty(e.Task.Error))
                {
                    Config.LogWriter.LogMessage(LogLevel.Error, e.Task.Error);
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

        #region Dispatcher state

        public override void Start()
        {
            SetState(true);
        }

        public override void Stop()
        {
            SetState(false);
        }

        #endregion
    }
}
