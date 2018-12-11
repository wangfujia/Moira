using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BAFactory.Moira.Core.Elements
{
    public class LogFileConfiguration
    {
        public FileInfo FilePath { get; set; }
        public RotateOptions RotateOption { get; set; }
        public ulong RotateLimit { get; set; }
        public ulong MaxFiles { get; set; }
    }
}
