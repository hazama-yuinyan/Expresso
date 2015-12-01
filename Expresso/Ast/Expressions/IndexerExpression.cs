using System;
using System.Collections.Generic;
using ICSharpCode.NRefactory;


namespace Expresso.Ast
{
    /// <summary>
    /// インデクサ式
    /// Represents an indexer expression.
    /// An indexer expression peeks at a value which sits on a certain position in an array.
    /// Target '[' { Expression } ']' ;
    /// </summary>
    public class IndexerExpression : Expression
    {
        public override TextLocation StartLocation{
            get{
                var first_item = Arguments.FirstOrNullObject();
                return first_item.IsNull ? base.StartLocation : first_item.StartLocation;
            }
        }

        public override TextLocation EndLocation{
            get{
                var last_item = Arguments.LastOrNullObject();
                return last_item.IsNull ? base.EndLocation : last_item.EndLocation;
            }
        }

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

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as IndexerExpression;
            return o != null && Target.DoMatch(o.Target, match) && Arguments.DoMatch(o.Arguments, match);
        }

        #endregion
    }
}

