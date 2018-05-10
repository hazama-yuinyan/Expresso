using System;
using System.Collections.Generic;

using System.Linq;
using ICSharpCode.NRefactory;

namespace Expresso.Ast
{
	/// <summary>
    /// 変数宣言文。
    /// The variable declaration statement.
    /// A variable declaration statement introduces a new variable or an immutable one into the surrounding scope.
    /// ("let" | "var") VariableInitializer { ',' VariableInitializer } ;
	/// </summary>
    public class VariableDeclarationStatement : Statement
	{
		/// <summary>
        /// 定義される変数郡。
        /// Variables that will be declared. Each node represents the variable declared and
        /// the corresponding initialization code.
		/// </summary>
        public AstNodeCollection<VariableInitializer> Variables => GetChildrenByRole(Roles.Variable);

        /// <summary>
        /// Gets or sets the modifiers.
        /// </summary>
        public Modifiers Modifiers{
            get{return EntityDeclaration.GetModifiers(this);}
            set{EntityDeclaration.SetModifiers(this, value);}
        }

        public ExpressoTokenNode SemicolonToken => GetChildByRole(Roles.SemicolonToken);

        public VariableDeclarationStatement(IEnumerable<PatternWithType> lhs, IEnumerable<Expression> rhs,
            Modifiers modifiers, TextLocation start, TextLocation end)
            : base(start, end)
        {
            foreach(var items in lhs.Zip(rhs, (l, r) => new Tuple<PatternWithType, Expression>(l, r)))
                Variables.Add(new VariableInitializer(items.Item1, items.Item2));
            
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

