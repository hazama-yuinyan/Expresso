using System;
using System.Collections.Generic;


namespace Expresso.Ast
{
    /// <summary>
    /// インデクサ式
    /// Represents an indexer expression.
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
            get{return GetChildByRole(Roles.TargetExpression);}
            set{SetChildByRole(Roles.TargetExpression, value);}
        }

        public ExpressoTokenNode LBracket{
            get{return GetChildByRole(Roles.LBracketToken);}
        }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        /// <value>The arguments.</value>
        public AstNodeCollection<Expression> Arguments{
            get{return GetChildrenByRole(Roles.Argument);}
        }

        public ExpressoTokenNode RBracket{
            get{return GetChildByRole(Roles.RBracketToken);}
        }

        public IndexerExpression(Expression target, IEnumerable<Expression> arguments)
        {
            Target = target;
            if(arguments != null)
                Arguments.AddRange(arguments);
        }

        public IndexerExpression(Expression target, params Expression[] arguments)
            : this(target, (IEnumerable<Expression>)arguments)
        {
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

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as IndexerExpression;
            return o != null && Target.DoMatch(o.Target, match) && Arguments.DoMatch(o.Arguments, match);
        }

        #endregion
    }
}

