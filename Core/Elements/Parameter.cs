using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using BAFactory.Moira.FileAnalyzers;
using System.Threading.Tasks;

namespace BAFactory.Moira.Core.Elements
{
    public class Parameter : ICloneable
    {
        public FileAttribute Attribute { get; set; }
        public bool IsFromLastResult { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        public Parameter()
        {
            Attribute = new FileAttribute();
            Name = string.Empty;
            Value = string.Empty;
            IsFromLastResult = false;
        }

        public static bool ExpandParameter(FileInfo f, ref Parameter p)
        {
            string format = GetPropertyFormat(p.Value);

            p.Attribute.Analyzer.Configure(f);
            string attribute = p.Attribute.Analyzer.GetAttribute(p.Attribute, format);

            if (string.IsNullOrEmpty(attribute))
            {
                return false;
            }

            string value = ReplaceParameter(p.Value, attribute);

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            p.Value = value;
            return true;
        }

        private static string GetPropertyFormat(string parameter)
        {
            Regex regEx = new Regex(@"\{(.*)\}");
            Match match = regEx.Match(parameter);

            if (match.Success && match.Groups.Count > 1)
            {
                return match.Groups[1].Value;
            }

            return string.Empty;
        }

        private static string ReplaceParameter(string parameter, string attribute)
        {
            if (string.IsNullOrEmpty(attribute))
            {
                return string.Empty;
            }
            string replaced = string.Empty;

            Regex regx = new Regex("{(.*)}");

            replaced = regx.Replace(parameter, attribute);

            return replaced;
        }

        #region ICloneable Members

        public object Clone()
        {
            Parameter clone = new Parameter();

            clone.Name = (string)this.Name.Clone();
            clone.Attribute = (FileAttribute)this.Attribute.Clone();
            clone.IsFromLastResult = this.IsFromLastResult;
            clone.Value = (string)this.Value.Clone();

            return clone;
        }

        #endregion
    }
}
