using System;
using System.Collections.Generic;

using Expresso.Compiler;
using System.Linq;

namespace Expresso.Ast
{
	/// <summary>
    /// 変数宣言文。
    /// The variable declaration.
    /// "let" | "var" Name ["(-" Type] {',' Name ["(-" Type]}
	/// </summary>
    public class VariableDeclarationStatement : Expression
	{
		/// <summary>
        /// 定義される変数郡。
        /// Variables that will be declared. Each node represents the variable declared and
        /// the corresponding initialization code.
		/// </summary>
        public AstNodeCollection<VariableInitializer> Variables{
            get{return GetChildrenByRole(Roles.Variable);}
		}

        /// <summary>
        /// Gets or sets the modifiers.
        /// </summary>
        /// <value>The modifiers.</value>
        public Modifiers Modifiers{
            get{return EntityDeclaration.GetModifiers(this);}
            set{EntityDeclaration.SetModifiers(this, value);}
        }

        public ExpressoTokenNode SemicolonToken{
            get{return GetChildByRole(Roles.SemicolonToken);}
        }

        public override NodeType NodeType{
            get{return NodeType.Statement;}
        }

        public VariableDeclarationStatement(Identifier lhs, Expression rhs, Modifiers modifiers)
		{
            AddChild(new VariableInitializer(lhs, rhs), Roles.Variable);
            Modifiers = modifiers;
		}

        public VariableDeclarationStatement(IEnumerable<Identifier> lhs, IEnumerable<Expression> rhs, Modifiers modifiers)
        {
            foreach(var items in Enumerable.Zip(lhs, rhs, (left, right) => new Tuple<Identifier, Expression>(left, right)))
                AddChild(new VariableInitializer(items.Item1, items.Item2), Roles.Variable);
            
            Modifiers = modifiers;
        }

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitVariableDeclarationStatement(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitVariableDeclarationStatement(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitVariableDeclarationStatement(this, data);
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as VariableDeclarationStatement;
            return o != null && Variables.DoMatch(o.Variables, match);
        }

        #endregion
	}
}

