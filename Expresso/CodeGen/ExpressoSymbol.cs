using System;
using System.Reflection;
using System.Linq.Expressions;


namespace Expresso.CodeGen
{
    /// <summary>
    /// Describes an entry to the emitter symbol table.
    /// </summary>
    public class ExpressoSymbol
    {
        public ParameterExpression Parameter{
            get; set;
        }

        public MethodInfo Method{
            get; set;
        }

        public FieldInfo Field{
            get; set;
        }

        public Type Type{
            get; set;
        }
    }
}

