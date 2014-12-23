using System;
using System.Collections.Generic;
using System.Linq;

using Expresso.Runtime;
using Expresso.Compiler;
using Expresso.Compiler.Meta;

namespace Expresso.Ast
{
    /// <summary>
    /// デフォルト式。C#の同名の文法要素と同じ働きをする。
	/// Reperesents a default expression in the sense of the C#'s default expression.
    /// </summary>
    public class DefaultExpression : Expression
    {
        public AstType TargetType{
            get{return GetChildByRole(Roles.Type);}
		}

        public override NodeType NodeType{
            get{
                return NodeType.Expression;
            }
        }

        public DefaultExpression(AstType targetType)
		{
            AddChild(targetType, Roles.Type);
		}

        public override bool Equals(object obj)
        {
            var x = obj as DefaultExpression;

            if(x == null)
                return false;

            return TargetType == x.TargetType;
        }

        public override int GetHashCode()
        {
            return TargetType.GetHashCode();
        }

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitDefaultExpression(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitDefaultExpression(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitDefaultExpression(this, data);
        }
    }
}
