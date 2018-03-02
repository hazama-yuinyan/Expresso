using System;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Expresso.CodeGen
{
    /// <summary>
    /// Describes an entry to the emitter symbol table.
    /// ExpressoSymbols bind Expresso world identifiers to CLR functions, methods, properties and fields.
    /// </summary>
    public class ExpressoSymbol
    {
        /// <summary>
        /// Represents a parameter.
        /// </summary>
        public ParameterExpression Parameter{
            get; set;
        }

        /// <summary>
        /// Represents a native method.
        /// </summary>
        public MethodInfo Method{
            get; set;
        }

        /// <summary>
        /// Represents a property or a field.
        /// </summary>
        public MemberInfo PropertyOrField{
            get; set;
        }

        /// <summary>
        /// Represents a method or a function.
        /// </summary>
        public LambdaExpression Lambda{
            get; set;
        }

        /// <summary>
        /// Represents a member.
        /// </summary>
        /// <value>The member.</value>
        public MemberExpression Member{
            get; set;
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        public Type Type{
            get; set;
        }

        /// <summary>
        /// Gets or sets the type builder.
        /// </summary>
        /// <value>The type builder.</value>
        public LazyTypeBuilder TypeBuilder{
            get; set;
        }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        /// <value>The parameters.</value>
        public List<ParameterExpression> Parameters{
            get; set;
        }

        public override string ToString()
        {
            return string.Format("[ExpressoSymbol: Parameter={0}, Method={1}, Field={2}, Lambda={3}, Member={4}, Type={5}, Length of Parameters={6}]", Parameter, Method, PropertyOrField, Lambda, Member, Type, Parameters.Count);
        }
    }
}

