using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FUnreal
{
    public abstract class AXAttrValidator : Attribute
    {
        public abstract string Description();
        public abstract bool Validate(object value);
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class XNotNullAttrValidator : AXAttrValidator
    {
        public override string Description()
        {
            return "Not Null";
        }

        public override bool Validate(object value)
        {
            return value != null;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class XStringContrainedValueAttrValidator : AXAttrValidator
    {
        private List<string> _constraints;

        public XStringContrainedValueAttrValidator(params string[] constraints)
        {
            _constraints = constraints.ToList();
        }

        public override string Description()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("One of [");
            for(int  i = 0; i < _constraints.Count; ++i)
            {
                builder.Append(_constraints[i]);
                if (i < _constraints.Count - 1) builder.Append(", ");
            }
            builder.Append("]");
            return builder.ToString();
        }


        public override bool Validate(object value)
        {
            if (!(value is string)) throw new ArgumentException("value is not a string but is " + value.GetType().Name);
            if (!_constraints.Contains(value as string)) return false;
            return true;
        }
    }

}
