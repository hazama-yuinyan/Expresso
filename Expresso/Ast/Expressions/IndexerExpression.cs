using System;
using System.Collections.Generic;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;


namespace Expresso.Ast
{
    /// <summary>
    /// インデクサ式
    /// Represents an indexer expression.
    /// An indexer expression peeks at a value which sits in a certain position of an array or at values which sits in a range of the array.
    /// Target '[' { Expression } ']' ;
    /// </summary>
    public class IndexerExpression : Expression
    {
        /// <summary>
        /// インデクシングを行う対象となるオブジェクトを生成する式。
        /// The expression to be evaluated and to be indexed.
        /// </summary>
        /// <value>The target.</value>
        public Expression Target{
            get => GetChildByRole(Roles.TargetExpression);
            set => SetChildByRole(Roles.TargetExpression, value);
        }

        public ExpressoTokenNode LBracket => GetChildByRole(Roles.LBracketToken);

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        /// <value>The arguments.</value>
        public AstNodeCollection<Expression> Arguments => GetChildrenByRole(Roles.Argument);

        public ExpressoTokenNode RBracket => GetChildByRole(Roles.RBracketToken);

        public IndexerExpression(Expression target, IEnumerable<Expression> arguments, TextLocation loc)
            : base(target.StartLocation, loc)
        {
            Target = target;
            if(arguments != null)
                Arguments.AddRange(arguments);
        }

        #region implemented abstract members of AstNode

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitIndexerExpression(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitIndexerExpression(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitIndexerExpression(this, data);
        }

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as IndexerExpression;
            return o != null && Target.DoMatch(o.Target, match) && Arguments.DoMatch(o.Arguments, match);
        }

        #endregion
    }
}

