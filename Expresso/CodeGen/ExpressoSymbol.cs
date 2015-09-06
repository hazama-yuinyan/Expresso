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

