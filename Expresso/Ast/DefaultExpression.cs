using System;
using System.Collections.Generic;
using System.Linq;

using Expresso.Interpreter;
using Expresso.Runtime;
using Expresso.Compiler;
using Expresso.Compiler.Meta;
using Expresso.Runtime.Operations;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

    /// <summary>
    /// デフォルト式。C#の同名の文法要素と同じ働きをする。
	/// Reperesents a default expression in the sense of the C#'s default expression.
    /// </summary>
    public class DefaultExpression : Expression
    {
		readonly TypeAnnotation target;

		public TypeAnnotation TargetType{
			get{return target;}
		}

		public DefaultExpression(TypeAnnotation targetType)
		{
			target = targetType;
		}

        public override NodeType Type{
            get{return NodeType.DefaultExpression;}
        }

        public override bool Equals(object obj)
        {
            var x = obj as DefaultExpression;

            if(x == null)
                return false;

            return target == x.target;
        }

        public override int GetHashCode()
        {
            return target.GetHashCode();
        }

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(ExpressoWalker walker)
		{
			if(walker.Walk(this)){}
			walker.PostWalk(this);
		}

		public override string ToString()
		{
            return string.Format("[Default expression for {0}]", TargetType.ObjType);
		}
    }
}
