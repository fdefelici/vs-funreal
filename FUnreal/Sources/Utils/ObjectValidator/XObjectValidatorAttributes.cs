using System;
using System.Collections.Generic;
using System.Linq;

namespace FUnreal
{
    public abstract class AXAttrValidator : Attribute
    {
        public abstract bool Validate(object value);
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class XNotNullAttrValidator : AXAttrValidator
    {
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

        public override bool Validate(object value)
        {
            if (!(value is string)) throw new ArgumentException("value is not a string but is " + value.GetType().Name);
            if (!_constraints.Contains(value as string)) return false;
            return true;
        }
    }

}
