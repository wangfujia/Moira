using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BAFactory.Moira.Core.Elements
{
    public enum BreakCondition
    {
        Never,
        OnError,
        OnSuccess,
        //OnPreviousError,
        //OnPreviousSuccess,
        Always,
    }
}
