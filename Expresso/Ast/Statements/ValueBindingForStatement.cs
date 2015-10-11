using System;

using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;
using System.Collections.Generic;

namespace Expresso.Ast
{
	/// <summary>
	/// 変数初期化型For文。
	/// Another For statement that introduces new variables into its child scope.
    /// A for statement is used for iterating over sequence-like objects.
    /// "for" ("let" | "var") PatternConstruct "in" Expression '{' Body '}' ;
	/// </summary>
    public class ValueBindingForStatement : Statement
	{
        public ExpressoTokenNode ForToken{
            get{return GetChildByRole(Roles.ForToken);}
        }

        public ExpressoTokenNode LPar{
            get{return GetChildByRole(Roles.LParenthesisToken);}
        }

        /// <summary>
        /// Gets or sets the modifiers.
        /// </summary>
        /// <value>The modifiers.</value>
        public Modifiers Modifiers{
            get{return EntityDeclaration.GetModifiers(this);}
            set{EntityDeclaration.SetModifiers(this, value);}
        }

		/// <summary>
        /// body内で操作対象となるオブジェクトを参照するのに使用する変数群。
        /// なお、走査の対象を捕捉する際には普通の代入と同じルールが適用される。
        /// つまり、複数の変数にいっせいにオブジェクトを捕捉させることもできる。
        /// When evaluating the both sides of the "in" keyword,
        /// the same rule as the assignment applies.
        /// So for example,
        /// for(let (x, y) in [1,2,3,4,5,6])...
        /// the x and y captures the first and second element of the list at the first time,
        /// the third and forth the next time, and the fifth and sixth the last time.
        /// </summary>
        public AstNodeCollection<VariableInitializer> Variables{
            get{return GetChildrenByRole(Roles.Variable);}
        }

        /// <summary>
        /// 操作対象のオブジェクトが存在する間評価し続けるブロック。
        /// The block we'll continue to evaluate until the sequence is ate up.
        /// </summary>
        public BlockStatement Body{
            get{return GetChildByRole(Roles.Body);}
            set{SetChildByRole(Roles.Body, value);}
        }

        public ValueBindingForStatement(Modifiers modifiers, IEnumerable<VariableInitializer> initializers,
            BlockStatement body, TextLocation loc)
            : base(loc, body.EndLocation)
        {
            Modifiers = modifiers;
            foreach(var initializer in initializers)
                AddChild(initializer, Roles.Variable);
            
            Body = body;
        }

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitValueBindingForStatement(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitValueBindingForStatement(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitValueBindingForStatement(this, data);
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as ValueBindingForStatement;
            return o != null && Variables.DoMatch(o.Variables, match) && Body.DoMatch(o.Body, match);
        }

        #endregion
	}
}

