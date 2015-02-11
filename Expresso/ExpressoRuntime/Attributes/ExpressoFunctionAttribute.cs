using System;

namespace Expresso.Runtime
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ExpressoFunctionAttribute : Attribute
    {
        readonly string name;

        public string ExposedName{
            get{return name;}
        }

        public ExpressoFunctionAttribute(string name)
        {
            this.name = name;
        }
    }
}

