using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;

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
        public ExpressoTokenNode ForToken => GetChildByRole(Roles.ForToken);

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
        /// If you want to destructure thr right-hand-side,
        /// the iterator has to produce a tuple.
        /// The iterator must return a single value or a tuple.
        /// </summary>
        public VariableInitializer Initializer{
            get{return GetChildByRole(Roles.Variable);}
            set{SetChildByRole(Roles.Variable, value);}
        }

        /// <summary>
        /// 操作対象のオブジェクトが存在する間評価し続けるブロック。
        /// The block we'll continue to evaluate until the sequence is ate up.
        /// </summary>
        public BlockStatement Body{
            get{return GetChildByRole(Roles.Body);}
            set{SetChildByRole(Roles.Body, value);}
        }

        public ValueBindingForStatement(Modifiers modifiers, VariableInitializer init, BlockStatement body, TextLocation loc)
            : base(loc, body.EndLocation)
        {
            Modifiers = modifiers;
            Initializer = init;
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
            return o != null && Initializer.DoMatch(o.Initializer, match) && Body.DoMatch(o.Body, match);
        }

        #endregion
	}
}

