using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BAFactory.Moira.Core.Elements
{
    public abstract class AssemblyInformation
    {
        public AssemblyInformation(string className, string assembly, string publicToken, string version)
            : this(string.Empty, className, assembly, publicToken, string.Empty, version)
        {
        }
        public AssemblyInformation(string id, string className, string assembly, string publicToken, string culture, string version)
        {
            this.Id = id;
            this.Class = className;
            this.Assembly = assembly;
            this.PublicKeyToken = publicToken;
            this.Culture = culture;
            this.Version = version;
        }

        public string Id { get; private set; }
        public string Class { get; private set; }
        public string Assembly { get; private set; }
        public string PublicKeyToken { get; private set; }
        public string Culture { get; private set; }
        public string Version { get; private set; }
    }
}
