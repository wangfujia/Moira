using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;

using BAFactory.Moira.FileAnalyzers;

namespace BAFactory.Moira.Core.Elements
{
    public class FileAttribute : ICloneable
    {
        public IFileAnalyzer Analyzer { get; set; }
        public long Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public FileAttribute()
        {
            Id = 0;
            Name = string.Empty;
            Type = string.Empty;
            Value = string.Empty;
        }
        public object Clone()
        {
            FileAttribute clone = new FileAttribute();
            clone.Id = this.Id;
            clone.Analyzer = this.Analyzer;
            clone.Name = (string)this.Name.Clone();
            clone.Type = (string)this.Type.Clone();
            clone.Value = (string)this.Value.Clone();
            return clone;
        }
    }
}
