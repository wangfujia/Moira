using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BAFactory.Moira.Core.Elements
{
    public class Parameters : Dictionary<string, Parameter>,ICloneable
    {

        #region ICloneable Members

        public object Clone()
        {
            Parameters clone = new Parameters();
            foreach (string key in this.Keys)
            {
                Parameter p = (Parameter)this[key].Clone();
                clone.Add(key, p);
            }
            return clone;
        }

        #endregion
    }
}
