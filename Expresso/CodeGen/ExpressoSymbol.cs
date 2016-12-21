using System;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Generic;
using Expresso.Ast;


namespace Expresso.CodeGen
{
    /// <summary>
    /// Describes an entry to the emitter symbol table.
    /// </summary>
    public class ExpressoSymbol
    {
        static Dictionary<string, Identifier> _Identifiers = new Dictionary<string, Identifier>{
            {"int", AstNode.MakeIdentifier("int")},
            {"uint", AstNode.MakeIdentifier("uint")},
            {"bool", AstNode.MakeIdentifier("bool")},
            {"float", AstNode.MakeIdentifier("float")},
            {"double", AstNode.MakeIdentifier("double")},
            {"bigint", AstNode.MakeIdentifier("bigint")},
            {"tuple", AstNode.MakeIdentifier("tuple")},
            {"vector", AstNode.MakeIdentifier("vector")},
            {"dictionary", AstNode.MakeIdentifier("dictionary")},
            {"array", AstNode.MakeIdentifier("array")},
            {"char", AstNode.MakeIdentifier("char")},
            {"string", AstNode.MakeIdentifier("string")},
            {"byte", AstNode.MakeIdentifier("byte")},
            {"function", AstNode.MakeIdentifier("function")},
            {"intseq", AstNode.MakeIdentifier("intseq")}
        };

        public static Dictionary<string, Identifier> Identifiers{
            get{return _Identifiers;}
        }

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
        /// Gets or sets the field.
        /// </summary>
        public FieldInfo Field{
            get; set;
        }

        /// <summary>
        /// Represents a method or a function.
        /// </summary>
        public LambdaExpression Lambda{
            get; set;
        }

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
            return string.Format("[ExpressoSymbol: Parameter={0}, Method={1}, Field={2}, Lambda={3}, Member={4}, Type={5}, Length of Parameters={6}]", Parameter, Method, Field, Lambda, Member, Type, Parameters.Count);
        }
    }
}

