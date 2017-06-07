using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    public class DescriptionAttribute : Attribute
    {
        public string Description
        {
            get;
            protected set;
        }

        public DescriptionAttribute(string value)
        {
            this.Description = value;
        }
    }
}
