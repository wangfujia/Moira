using System;
using System.Threading;
using System.Timers;

namespace BAFactory.Moira.Core
{
    public abstract class Dispatcher
    {
        protected System.Timers.Timer MainTimer { get; set; }

        protected static ConfigurationProvider Config { get; set; }

        public abstract bool IsEnabled { get; }
        public ConfigurationProvider Configuration
        {
            get
            {
                return Dispatcher.Config;
            }
            set
            {
                Dispatcher.Config = value;
            }
        }
        public static Dispatcher GetDispatcher()
        {
            return new ConfigurationProvider().Dispatcher;
        }
        public virtual void Start()
        {
            RunOnStartup();

            StartTimer();

            SetState(true);


            while (true)
            {
                Thread.Sleep(int.MaxValue);
            }
        }
        public virtual void Stop()
        {
            SetState(false);
        }
        protected void SetState(bool e)
        {
            MainTimer.Enabled = e;

            if (e)
            {
                MainTimer.Start();
            }
            else
            {
                MainTimer.Stop();
            }
        }
        protected void StartTimer()
        {
            MainTimer.Elapsed += new ElapsedEventHandler(AttendTimer);
            MainTimer.Enabled = true;
        }
        protected abstract void AttendTimer(object sender, ElapsedEventArgs e);
        protected abstract void RunOnStartup();
    }
}
