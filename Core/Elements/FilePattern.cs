using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BAFactory.Moira.Core.Elements
{
    class FilePattern
    {
        private string pattern;
        private bool isRegEx;
        public string Pattern
        {
            get { return pattern; }
            set { pattern = value; }
        }
        public bool IsRegEx
        {
            get { return isRegEx; }
            set { isRegEx = value; }
        }
    }
}
