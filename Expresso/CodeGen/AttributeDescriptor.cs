using System;
using System.Reflection;

namespace Expresso.CodeGen
{
    /// <summary>
    /// Describes arguments for the constructor of <see cref="System.Reflection.Emit.CustomAttributeBuilder"/>
    /// </summary>
    internal class AttributeDescriptor
    {
        public ConstructorInfo Constructor{
            get; set;
        }

        public object[] Arguments{
            get; set;
        }
    }
}
