using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BAFactory.Moira.Core;
using System.Threading;

namespace BAFActory.Moira
{
    class Program
    {
        static void Main(string[] args)
        {
            Dispatcher dispatcher = Dispatcher.GetDispatcher();

            try
            {
                dispatcher.Start();

            }
            catch (Exception e)
            {
                e.ToString();
            }
        }
    }
}
