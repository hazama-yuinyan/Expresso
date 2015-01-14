using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Expresso.Compiler;
using Expresso.Compiler.Meta;

namespace Expresso.Ast
{
    /// <summary>
    /// 識別子。
	/// Reperesents a symbol.
    /// </summary>
    public class Identifier : AstNode
    {
        public static new Identifier Null = new NullIdentifier();
        sealed class NullIdentifier : Identifier
        {
            public override bool IsNull{
                get{
                    return true;
                }
            }

            public override void AcceptWalker(IAstWalker walker)
            {
                walker.VisitNullNode(this);
            }

            public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
            {
                return walker.VisitNullNode(this);
            }

            public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
            {
                return walker.VisitNullNode(this, data);
            }

            internal protected override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
            {
                return other == null || other.IsNull;
            }
        }

		readonly string name;

        /// <summary>
        /// 識別子名。
		/// The name of the identifier.
        /// </summary>
        public string Name{
            get{return name;}
		}

        public bool IsVerbatim{
            get{
                return true;
            }
        }

		/// <summary>
		/// 変数の型。
		/// The type of the variable.
		/// </summary>
        public AstType Type{
            get{return GetChildByRole(Roles.Type);}
            set{SetChildByRole(Roles.Type, value);}
		}

        public override NodeType NodeType{
            get{return NodeType.Unknown;}
        }

        public Identifier(string name)
		{
            this.name = name;
		}

        public Identifier(string name, AstType type)
        {
            this.name = name;
            Type = type;
        }

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitIdentifier(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitIdentifier(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitIdentifier(this, data);
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as Identifier;
            return o != null && Name == o.Name && Type.DoMatch(o.Type);
        }

        #endregion

        /*internal override void Assign(EvaluationFrame frame, object val)
		{
			if(Reference != null)
				frame.Assign(this.Offset, val);
			else
				throw ExpressoOps.MakeRuntimeError("Unbound name: {0}", name);
		}*/
    }
}
