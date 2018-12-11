using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BAFactory.Moira.Core.Elements
{
    internal class Timetable : List<Timetable.Entry>
    {
        internal Timetable()
        {
        }

        internal class Entry
        {
            internal Job Task { get; set; }
            internal bool IsExecuting { get; set; }
            public ulong NextRun { get; set; }
        }
    }
}
