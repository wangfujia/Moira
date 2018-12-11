using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAFactory.Moira.Core.Log
{
    public interface ILogProvider : IDisposable
    {
        void InitializeLogProvider();
        void LogMessage(LogLevel l, string message);
        Task LogMessageAsync(LogLevel l, string message);
        void LogException(Exception e);
    }
}
