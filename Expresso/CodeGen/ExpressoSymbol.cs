using System;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Reflection.Emit;

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
        /// Represents a field builder.
        /// It's for referencing fields in the CSharpEmitter class.
        /// Don't use it for actually referencing the fields because otherwise we can't reference fields that
        /// have the belonging class as its type.
        /// </summary>
        /// <value>The field builder.</value>
        public FieldBuilder FieldBuilder{
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
            return string.Format("[ExpressoSymbol: Parameter={0}, Method={1}, PropertyOrField={2}, FieldBuilder={3}, Lambda={4}, Member={5}, Type={6}, Length of Parameters={7}]", Parameter, Method, PropertyOrField, FieldBuilder, Lambda, Member, Type, Parameters.Count);
        }
    }
}

